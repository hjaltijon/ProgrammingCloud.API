using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProgrammingCloud.API.BusinessLogic;
using ProgrammingCloud.API.Configuration;
using ProgrammingCloud.API.Helpers;
using ProgrammingCloud.API.Models.DTOs;
using ProgrammingCloud.API.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class ProblemsController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly AppSettings _settings;
        private readonly IMapper _mapper;
        private readonly UserBL _userBL;
        private readonly ProblemBL _problemBL;

        public ProblemsController(
            UserBL userBL,
            ProblemBL problemBL,
            IHttpClientFactory clientFactory, 
            IMapper mapper,
            IOptions<AppSettings> settings)
        {
            _userBL = userBL;
            _problemBL = problemBL;
            _clientFactory = clientFactory;
            _mapper = mapper;
            _settings = settings.Value;
        }


        [Authorize]
        [HttpPost("compile-run", Name = nameof(CompileAndRun))]
        public async Task<IActionResult> CompileAndRun([FromBody] CompilerInfoDTO dto)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(CompileAndRun));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }
            
            var request = new HttpRequestMessage(HttpMethod.Post, "https://function-lokaverk-1.azurewebsites.net/api/CompileAndRunCodeFunction");
            //var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:7071/api/CompileAndRunCodeFunction");

            if (dto.ProblemId == null)
            {
                return BadRequest();
            }
            //TODO: CAN USER RUN FOR THIS PROBLEM?
            string combinedCode = await _problemBL.CombineStudentAndTestingCode(dto.StudentCode, (int)dto.ProblemId);

            request.Headers.Add("x-functions-key", _settings.FunctionSecret);
            //string json = JsonSerializer.Serialize(combinedCode);
            request.Content = new StringContent(combinedCode, Encoding.UTF8, "application/json");

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);
            string responseString = "";

            if (response.IsSuccessStatusCode)
            {
                responseString = await response.Content.ReadAsStringAsync();
            }
            else
            {
                return StatusCode(500); //server error
            }

            var result = JsonSerializer.Deserialize<CodeExecutionResult>(responseString);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("compile", Name = nameof(Compile))]
        public async Task<IActionResult> Compile([FromBody] CompilerInfoDTO dto)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(Compile));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            //if (dto.ProblemId == null)
            //{
            //    return BadRequest();
            //}
            //TODO: CAN USER RUN FOR THIS PROBLEM?

            List<CompilerError> compilerErrors = await _problemBL.Compile(dto);

            return Ok(compilerErrors);
        }




        /// <summary>
        /// Create Problem
        /// </summary>
        /// <remarks>
        /// AccessLevels:
        /// 8 - Caller can create a Problem.
        /// </remarks>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal server error</response>
        /// <param name="problemDTO">The resource to be created</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ProblemDTO), 201)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpPost("problems")]
        [Authorize]
        public async Task<IActionResult> CreateProblem
            ([FromBody] ProblemForCreationDTO problemDTO)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(CreateProblem));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (problemDTO == null)
            {
                return BadRequest();
            }


            var problem = _mapper.Map<Problem>(problemDTO);
            problem.UserId = int.Parse(User.GetUserId());
            TryValidateModel(problem);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var problemId = await _problemBL.CreateProblem(problem);
            var problemToReturn = _mapper.Map<ProblemDTO>
                (await _problemBL.GetProblem(problemId));

            return CreatedAtRoute(nameof(GetProblem),
                new { problemId = problemId },
                problemToReturn);
        }


        /// <summary>
        /// Update Problem
        /// </summary>
        /// <remarks>
        /// This PATCH method does NOT support upsert semantics, you can only use it to update a resource <br/>
        /// For info on PATCH requests see: http://jsonpatch.com/ 
        /// <br/><br/>
        /// AccessLevels: <br/>
        /// 8 - Caller can update his own Problems.
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        /// <response code="500">Internal server error</response>
        /// <param name="problemId">id of the problem</param>
        /// <param name="patchDoc">json patch document</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ProblemDTO), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 409)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpPatch("problems/{problemId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProblem
            ([FromRoute] int? problemId,
            [FromBody] JsonPatchDocument<ProblemForUpdateDTO> patchDoc)
        {
            //string callerUsername = User.GetUsername();

            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(UpdateProblem));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (patchDoc == null || problemId == null)
            {
                return BadRequest();
            }

            var problemFromRepo =
                await _problemBL.GetProblem((int)problemId);

            //because the db creates the resource Id
            //we do not support upserting here, POST should be used
            //to create a resource when the user is not supposed to
            //create the resource id himself
            if (problemFromRepo == null)
            {   //409 instead of 404 according to microsoft rest api guidelines
                return StatusCode(409, "Resource does not exist");
            }

            var problemToPatch = _mapper.Map<ProblemForUpdateDTO>(problemFromRepo);
            patchDoc.ApplyTo(problemToPatch, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(problemToPatch, problemFromRepo);
            TryValidateModel(problemFromRepo);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _problemBL.CanCallerUpdateProblem(problemFromRepo, User, callerActionAccessLevel))
            {
                return Unauthorized();
            }

            await _problemBL.UpdateProblem(problemFromRepo);            

            //return the updated problem
            var problemToReturn = _mapper.Map<ProblemDTO>
                (await _problemBL.GetProblem((int)problemId));
            return Ok(problemToReturn);
        }



        /// <summary>
        /// Get Problem by id
        /// </summary>
        /// <remarks>
        /// AccessLevels: 
        /// 8 - Caller can get a Problem related to him.
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal server error</response>
        /// <param name="problemId">id of Problem</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ProblemDTO), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet("problems/{problemId}", Name = nameof(GetProblem))]
        [Authorize]
        public async Task<IActionResult> GetProblem([FromRoute] int? problemId)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(GetProblem));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (problemId == null)
            {
                return BadRequest();
            }

            var problem = await _problemBL.GetProblem((int)problemId);
            if (problem == null)
            {
                return NotFound();
            }

            if (!await _problemBL.CanCallerGetProblem(problem, User, callerActionAccessLevel))
            {
                return Unauthorized();
            }
            return Ok(_mapper.Map<ProblemDTO>(problem));
        }



        /// <summary>
        /// Get Problems
        /// </summary>
        /// <remarks>
        /// AccessLevels: 
        /// 8 - Caller can get all Problems related to him.
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal server error</response>
        /// <param name="userId">id of User</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<ClassroomDTO>), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet("user/{userId}/problems", Name = nameof(GetProblems))]
        [Authorize]
        public async Task<IActionResult> GetProblems([FromRoute] int? userId)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(GetProblems));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (userId == null)
            {
                return BadRequest();
            }

            if (!await _problemBL.CanCallerGetAllProblemsRelatedToUser((int)userId, User, callerActionAccessLevel))
            {
                return Unauthorized();
            }

            var problems = await _problemBL.GetAllProblemsRelatedToUser((int)userId);
            if (problems == null)
            {
                return NotFound();
            }


            return Ok(_mapper.Map<List<ProblemDTO>>(problems));
        }




        /// <summary>
        /// Create ProblemClassroomRelation
        /// </summary>
        /// <remarks>
        /// AccessLevels:
        /// 8 - Caller can create a ProblemClassroomRelation for his Problems/Classrooms
        /// </remarks>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal server error</response>
        /// <param name="ProblemClassroomRelationDTO">The resource to be created</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ProblemClassroomRelationDTO), 201)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpPost("problem-classroom-relations")]
        [Authorize]
        public async Task<IActionResult> CreateProblemClassroomRelation
            ([FromBody] ProblemClassroomRelationDTO problemClassroomRelationDTO)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(CreateProblemClassroomRelation));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (problemClassroomRelationDTO == null)
            {
                return BadRequest();
            }


            var problemClassroomRelation = _mapper.Map<ProblemClassroomRelation>(problemClassroomRelationDTO);
            TryValidateModel(problemClassroomRelation);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _problemBL.CanCallerCreateProblemClassroomRelation(problemClassroomRelation, User, callerActionAccessLevel))
            {
                return Unauthorized();
            }

            await _problemBL.CreateProblemClassroomRelation(problemClassroomRelation);

            return StatusCode(201);

            //var userClassroomRelationToReturn = _mapper.Map<UserClassroomRelationDTO>
            //    (await _classroomBL.GetUserClassroomRelation(asdfasd fsdf sd));

            //return CreatedAtRoute(nameof(GetUserClassroomRelation),
            //    new { classroomId = asdf sadf, asdf sdf },
            //    userClassroomRelationToReturn);
        }



        /// <summary>
        /// Get Problems related to the specified Classroom
        /// </summary>
        /// <remarks>
        /// AccessLevels: 
        /// 8 - Caller can get all Classrooms related to his Classrooms.
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal server error</response>
        /// <param name="classroomId">id of Classroom</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<ClassroomDTO>), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet("classroom/{classroomId}/problems", Name = nameof(GetClassroomProblems))]
        [Authorize]
        public async Task<IActionResult> GetClassroomProblems([FromRoute] int? classroomId)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(GetClassroomProblems));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (classroomId == null)
            {
                return BadRequest();
            }

            if (!await _problemBL.CanCallerGetClassroomProblems((int)classroomId, User, callerActionAccessLevel))
            {
                return Unauthorized();
            }

            var problems = await _problemBL.GetClassroomProblems((int)classroomId);
            if (problems == null)
            {
                return NotFound();
            }


            return Ok(_mapper.Map<List<ProblemDTO>>(problems));
        }

    }
}
