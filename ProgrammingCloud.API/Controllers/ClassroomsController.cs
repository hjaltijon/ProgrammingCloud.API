using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProgrammingCloud.API.BusinessLogic;
using ProgrammingCloud.API.Helpers;
using ProgrammingCloud.API.Models.DTOs;
using ProgrammingCloud.API.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class ClassroomsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly UserBL _userBL;
        private readonly ClassroomBL _classroomBL;
        public ClassroomsController(UserBL userBL, ClassroomBL classroomBL, IMapper mapper)
        {
            _userBL = userBL;
            _classroomBL = classroomBL;
            _mapper = mapper;
        }








        /// <summary>
        /// Create Classroom
        /// </summary>
        /// <remarks>
        /// AccessLevels:
        /// 8 - Caller can create a Classroom.
        /// </remarks>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal server error</response>
        /// <param name="classroomDTO">The resource to be created</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ClassroomDTO), 201)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpPost("classrooms")]
        [Authorize]
        public async Task<IActionResult> CreateClassroom
            ([FromBody] ClassroomForCreationDTO classroomDTO)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(CreateClassroom));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (classroomDTO == null)
            {
                return BadRequest();
            }


            var classroom = _mapper.Map<Classroom>(classroomDTO);
            classroom.TeacherId = int.Parse(User.GetUserId());
            TryValidateModel(classroom);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //if (!await _classBL.CanCallerCreateProduct(classs, User, callerActionAccessLevel))
            //{
            //    return Unauthorized();
            //}

            var classroomId = await _classroomBL.CreateClassroom(classroom);

            var classroomToReturn = _mapper.Map<ClassroomDTO>
                (await _classroomBL.GetClassroom(classroomId));

            return CreatedAtRoute(nameof(GetClassroom),
                new { classroomId = classroomId },
                classroomToReturn);
        }


        /// <summary>
        /// Get Classroom by id
        /// </summary>
        /// <remarks>
        /// AccessLevels: 
        /// 8 - Caller can get any Classroom.
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal server error</response>
        /// <param name="classroomId">id of Classroom</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ClassroomDTO), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet("classrooms/{classroomId}", Name = nameof(GetClassroom))]
        [Authorize]
        public async Task<IActionResult> GetClassroom([FromRoute] int? classroomId)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(GetClassroom));
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

            var classroom = await _classroomBL.GetClassroom((int)classroomId);
            if (classroom == null)
            {
                return NotFound();
            }

            //if (!await _classroomBL.CanCallerGetClassroom(classroom, User, callerActionAccessLevel))
            //{
            //    return Unauthorized();
            //}
            return Ok(_mapper.Map<ClassroomDTO>(classroom));
        }

        /// <summary>
        /// Get Classrooms
        /// </summary>
        /// <remarks>
        /// AccessLevels: 
        /// 8 - Caller can get all Classrooms related to him.
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
        [HttpGet("user/{userId}/classrooms", Name = nameof(GetClassrooms))]
        [Authorize]
        public async Task<IActionResult> GetClassrooms([FromRoute] int? userId)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(GetClassrooms));
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

            if (!await _classroomBL.CanCallerGetAllClassroomsRelatedToUser((int)userId, User, callerActionAccessLevel))
            {
                return Unauthorized();
            }

            var classrooms = await _classroomBL.GetAllClassroomsRelatedToUser((int)userId);
            if (classrooms == null)
            {
                return NotFound();
            }

            
            return Ok(_mapper.Map<List<ClassroomDTO>>(classrooms));
        }

        /// <summary>
        /// Create UserClassroomRelation
        /// </summary>
        /// <remarks>
        /// AccessLevels:
        /// 8 - Caller can add users to his own Classroom
        /// </remarks>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal server error</response>
        /// <param name="classroomId">Classroom Id</param>
        /// <param name="email">Email</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserClassroomRelationDTO), 201)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpPost("classrooms/{classroomId}/users/{email}")]
        [Authorize]
        public async Task<IActionResult> CreateUserClassroomRelation
            ([FromRoute] int? classroomId, [FromRoute] string email)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(CreateUserClassroomRelation));
            if (callerActionAccessLevel == 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (classroomId == null || email == null)
            {
                return BadRequest();
            }

            if (!await _classroomBL.CanCallerCreateUserClassroomRelation((int)classroomId, User, callerActionAccessLevel))
            {
                return Unauthorized();
            }

            await _classroomBL.CreateUserClassroomRelation((int)classroomId, email);

            return StatusCode(201);

            //var userClassroomRelationToReturn = _mapper.Map<UserClassroomRelationDTO>
            //    (await _classroomBL.GetUserClassroomRelation(asdfasd fsdf sd));

            //return CreatedAtRoute(nameof(GetUserClassroomRelation),
            //    new { classroomId = asdf sadf, asdf sdf },
            //    userClassroomRelationToReturn);
        }

    }
}
