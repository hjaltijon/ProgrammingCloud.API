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
    public class UsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly UserBL _userBL;
        public UsersController(UserBL userBL, IMapper mapper)
        {
            _userBL = userBL;
            _mapper = mapper;
        }

        /// <summary>
        /// Request AccessToken
        /// </summary>
        /// <remarks>
        /// If the provided credentials are correct the response will contain an access token
        /// for the specified User. All API methods(except this one) require an Authorization header.
        /// The header must contain the access token and the authorization type(Bearer). For example:
        /// <br/>
        /// Authorization : Bearer access_token
        /// </remarks>
        /// <response code="204">No Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal server error</response>
        /// <param name="userCredentials">DTO containing user credentials</param>
        /// <param name="email">email of user</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpPost("users/{email}/access-token")]
        //[Throttle(Name = "RequestAccessToken", Seconds = 24 * 60 * 60, RequestCountLimit = 2500)]
        //TODO: THROTTLING!!
        public async Task<IActionResult> RequestAccessToken
            ([FromRoute] string email, [FromBody] UserCredentialsDTO userCredentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (email == null || userCredentials == null)
            {
                return BadRequest();
            }

            string accessToken = await _userBL.TryGetAccessToken(email, userCredentials.Password);
            if (accessToken == null) return Unauthorized();

            return Ok(new { accessToken = accessToken });
        }




        /// <summary>
        /// Get User by userId
        /// </summary>
        /// <remarks>
        /// AccessLevels:
        /// 8 - Caller can get any user.
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="userId">id of user</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserDTO), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet("users/{userId}", Name = nameof(GetUser))]
        [Authorize]
        public async Task<IActionResult> GetUser([FromRoute] int? userId)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(GetUser));
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
            User user;
            if (User.GetUserId() == userId.ToString())
            {
                user = await _userBL.GetUserAlongWithActionAccessMappings(User);
            }
            else
            {
                user = await _userBL.GetUser((int)userId);
            }

            if (user == null)
            {
                return NotFound();
            }

            //if (!await _userBL.CanCallerGetUser(user, User, callerActionAccessLevel))
            //{
            //    return Unauthorized();
            //}

            return Ok(_mapper.Map<UserDTO>(user));
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
        /// <param name="ActivateUserDTO">The resource to be created</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserDTO), 201)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpPost("user/{email}/activate")]
        public async Task<IActionResult> ActivateUser
            ([FromBody] ActivateUserDTO activateUserDTO, [FromRoute] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (activateUserDTO == null || email == null)
            {
                return BadRequest();
            }

            TryValidateModel(activateUserDTO);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            (bool authorized, int userId) result = await _userBL.ActivateUser(activateUserDTO);
            if (!result.authorized)
            {
                return Unauthorized();
            }

            var userToReturn = _mapper.Map<UserDTO>
                (await _userBL.GetUser(result.userId));

            return CreatedAtRoute(nameof(GetUser),
                new { userId = result.userId },
                userToReturn);
        }


        /// <summary>
        /// Get Users in Classroom
        /// </summary>
        /// <remarks>
        /// AccessLevels:
        /// 8 - Caller can get Users in a Classroom he owns.
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="classroomId">id of classroom</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserDTO), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet("classroom/{classroomId}/users", Name = nameof(GetClassroomUsers))]
        [Authorize]
        public async Task<IActionResult> GetClassroomUsers([FromRoute] int? classroomId)
        {
            int callerActionAccessLevel = await _userBL.GetCallerActionAccessLevel(User, nameof(GetClassroomUsers));
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

            if (!await _userBL.CanCallerGetClassroomUsers((int)classroomId, User, callerActionAccessLevel))
            {
                return Unauthorized();
            }

            var users = await _userBL.GetClassroomUsers((int)classroomId);

            

            return Ok(_mapper.Map<List<UserDTO>>(users));
        }
    }
}
