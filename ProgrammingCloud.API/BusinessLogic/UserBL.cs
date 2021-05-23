using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProgrammingCloud.API.Configuration;
using ProgrammingCloud.API.Helpers;
using ProgrammingCloud.API.Models.DataEntities;
using ProgrammingCloud.API.Models.DTOs;
using ProgrammingCloud.API.Models.Models;
using ProgrammingCloud.API.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.BusinessLogic
{
    public class UserBL
    {
        private readonly IMapper _mapper;
        private readonly UserRepository _userRepository;
        private readonly AppSettings _settings;
        private readonly ActionAccessMappings _actionAccessMappings;
        private readonly ClassroomRepository _classroomRepository;
        private readonly Cryptography _crypto;

        
        public UserBL(IMapper mapper,
            UserRepository userRepository,
            IOptions<AppSettings> settings,
            ActionAccessMappings actionAccessMappings,
            ClassroomRepository classroomRepository,
            Cryptography crypto)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _settings = settings.Value;
            _actionAccessMappings = actionAccessMappings;
            _classroomRepository = classroomRepository;
            _crypto = crypto;
        }



        public async Task<string> TryGetAccessToken(string email, string password)
        {
            UserEntity userEntity = await _userRepository.GetUserByEmail(email);

            if (userEntity != null && _crypto.ComputeHash(password, userEntity.Salt) == userEntity.PasswordHash)
            {
                var claims = new[] {
                    new Claim("userId", userEntity.UserId.ToString()),
                    new Claim("email", userEntity.Email),
                    new Claim("userTypeKey", userEntity.UserTypeKey),
                    new Claim("jti", Guid.NewGuid().ToString())
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtKey));
                var token = new JwtSecurityToken(
                    issuer: _settings.JwtIssuer,
                    audience: _settings.JwtIssuer,
                    expires: DateTime.UtcNow.AddHours(3),
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            return null;
            //var tokenHandler = new JwtSecurityTokenHandler();
            //var claims = new[] {
            //    new Claim(JwtRegisteredClaimNames.Sub, "12322"),
            //    new Claim(JwtRegisteredClaimNames.Email, "sampoekemail@example.com"),
            //    new Claim("UserTypeKey", "superadmin"),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            //};

            //var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtKey));
            //var token = new JwtSecurityToken(
            //    issuer: _settings.JwtIssuer,
            //    audience: _settings.JwtIssuer,
            //    expires: DateTime.UtcNow.AddHours(24),
            //    claims: claims,
            //    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            //);

            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(new Claim[]
            //    {
            //        new Claim(JwtRegisteredClaimNames.Sub, "12322"),
            //        new Claim(JwtRegisteredClaimNames.Email, "sampoekemail@example.com"),
            //        new Claim("UserTypeKey", "superadmin"),
            //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            //    }),
            //    Expires = DateTime.UtcNow.AddHours(24),
            //    Issuer = _settings.JwtIssuer,
            //    Audience = _settings.JwtIssuer,
            //    SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
            //};

            //var token2 = tokenHandler.CreateToken(tokenDescriptor);
            //return tokenHandler.WriteToken(token) + "-----------" + tokenHandler.WriteToken(token2);
        }

        public async Task<int> GetCallerActionAccessLevel(ClaimsPrincipal caller, string action)
        {
            return _actionAccessMappings.GetCallerActionAccessLevel(caller.GetUserTypeKey(), action);
        }

        public async Task<User> GetUserWithUnVerifiedEmail(string email)
        {
            var user = await _userRepository.GetUserWithUnVerifiedEmail(email);
            return _mapper.Map<User>(user);
        }

        public async Task<User> GetUserWithUnVerifiedEmail(int userId)
        {
            var user = await _userRepository.GetUserWithUnVerifiedEmail(userId);
            return _mapper.Map<User>(user);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            return _mapper.Map<User>(user);
        }

        public async Task<User> GetUser(int userId)
        {
            var user = await _userRepository.GetUserByUserId(userId);
            return _mapper.Map<User>(user);
        }

        public async Task RegisterVerifyEmailToken(int userId, string hashedVerifyEmailToken, DateTime verifyEmailTokenCreatedDate)
        {
            await _userRepository.RegisterVerifyEmailToken(userId, hashedVerifyEmailToken, verifyEmailTokenCreatedDate);
        }

        public async Task<User> GetUserAlongWithActionAccessMappings(ClaimsPrincipal caller)
        {
            int userId = int.Parse(caller.GetUserId());
            string userTypeKey = caller.GetUserTypeKey();

            Dictionary<string, int> aamappings = _actionAccessMappings.GetActionAccessMappings(userTypeKey);
            var userEntity = await _userRepository.GetUserByUserId(userId);
            var user = _mapper.Map<User>(userEntity);
            if (user != null)
            {
                user.ActionAccessMappings = aamappings;
            }
            return user;
        }

        public async Task<(bool authorized, int userId)> ActivateUser(ActivateUserDTO activateUserDTO)
        {
            var unverifiedUser = await GetUserWithUnVerifiedEmail(activateUserDTO.Email);
            if (unverifiedUser.VerifyEmailTokenHash != null &&
                unverifiedUser.VerifyEmailTokenCreatedDate != null &&
                unverifiedUser.VerifyEmailTokenCreatedDate < DateTime.UtcNow.AddDays(5) &&
                _crypto.ComputeSHA512Hash(activateUserDTO.VerifyEmailToken) == unverifiedUser.VerifyEmailTokenHash)
            {
                unverifiedUser.FullName = activateUserDTO.FullName;
                unverifiedUser.UserTypeKey = "student";
                unverifiedUser.Salt = _crypto.GenerateSalt();
                unverifiedUser.PasswordHash = _crypto.ComputeHash(activateUserDTO.Password, unverifiedUser.Salt);
                unverifiedUser.IsEmailVerified = true;
                unverifiedUser.VerifyEmailTokenHash = null;
                unverifiedUser.VerifyEmailTokenCreatedDate = null;
                await _userRepository.ActivateUser(_mapper.Map<UserEntity>(unverifiedUser));
                return (true, (int)unverifiedUser.UserId);
            }
            return (false, -1);
        }

        public async Task<bool> CanCallerGetClassroomUsers(int classroomId, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            int callerUserId = int.Parse(caller.GetUserId());
            if (callerActionAccessLevel == 8)
            {   //Caller can only get the students if he is the teacher of the classroom
                var classroom = await _classroomRepository.GetClassroom(classroomId);
                if (classroom.TeacherId == callerUserId)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<List<User>> GetClassroomUsers(int classroomId)
        {
            List<UserEntity> userEntities = await _userRepository.GetClassroomUsers(classroomId);
            return _mapper.Map<List<User>>(userEntities);
        }

        public async Task<int> CreateUnverifiedUser(string email, string hashedVerifyEmailToken, DateTime verifyEmailTokenCreatedDate)
        {
            int userId = await _userRepository.CreateUnverifiedUser(email, hashedVerifyEmailToken, verifyEmailTokenCreatedDate);
            return userId;
        }
    }
}
