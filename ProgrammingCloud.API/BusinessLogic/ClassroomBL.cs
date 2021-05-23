using AutoMapper;
using ProgrammingCloud.API.Helpers;
using ProgrammingCloud.API.Models.DataEntities;
using ProgrammingCloud.API.Models.Models;
using ProgrammingCloud.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.BusinessLogic
{
    public class ClassroomBL
    {
        private readonly IMapper _mapper;
        private readonly UserBL _userBL;
        private readonly ClassroomRepository _classroomRepository;
        private readonly EmailSender _emailSender;
        private readonly Cryptography _crypto;

        public ClassroomBL(IMapper mapper,
            UserBL userBL,
            ClassroomRepository classroomRepository,
            EmailSender emailSender,
            Cryptography crypto)
        {
            _mapper = mapper;
            _userBL = userBL;
            _classroomRepository = classroomRepository;
            _emailSender = emailSender;
            _crypto = crypto;
        }

        public async Task<int> CreateClassroom(Classroom classroom)
        {
            var classroomEntity = _mapper.Map<ClassroomEntity>(classroom);
            int classroomId = await _classroomRepository.CreateClassroom(classroomEntity);
            return classroomId;            
        }

        public async Task<Classroom> GetClassroom(int classroomId)
        {
            var classroomEntity = await _classroomRepository.GetClassroom(classroomId);
            return _mapper.Map<Classroom>(classroomEntity);
        }

        public async Task<bool> CanCallerGetAllClassroomsRelatedToUser(int userId, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            int callerUserId = int.Parse(caller.GetUserId());
            if (callerUserId == userId)
            {
                return true;
            }
            return false;
        }

        public async Task<List<Classroom>> GetAllClassroomsRelatedToUser(int userId)
        {
            List<ClassroomEntity> classroomEntities = await _classroomRepository.GetAllClassroomsRelatedToUser(userId);
            return _mapper.Map<List<Classroom>>(classroomEntities);
        }

        public async Task<bool> CanCallerCreateUserClassroomRelation(int classroomId, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            int callerUserId = int.Parse(caller.GetUserId());
            if (callerActionAccessLevel == 8)
            {
                var classroom = await GetClassroom(classroomId);
                if (callerUserId == classroom.TeacherId)
                {//caller can only add users to his own classroom
                    return true;
                }
            }
            return false;
        }
        public async Task CreateUserClassroomRelation(int classroomId, string email)
        {
            var user = await _userBL.GetUserByEmail(email);
            if (user != null)
            {
                await _classroomRepository.CreateUserClassroomRelation((int)user.UserId, classroomId);
                return;
            }
            else
            {
                var unverifiedUser = await _userBL.GetUserWithUnVerifiedEmail(email);
                string verifyEmailToken = _crypto.GenerateRandomString();
                string hashedVerifyEmailToken = _crypto.ComputeSHA512Hash(verifyEmailToken);
                DateTime verifyEmailTokenCreatedDate = DateTime.UtcNow;
                if (unverifiedUser != null)
                {
                    //change token & send email & create relation
                    await _userBL.RegisterVerifyEmailToken((int)unverifiedUser.UserId, hashedVerifyEmailToken, verifyEmailTokenCreatedDate);
                    await _emailSender.SendVerifyEmailEmail(email, (int)unverifiedUser.UserId, verifyEmailToken);
                    await _classroomRepository.CreateUserClassroomRelation((int)unverifiedUser.UserId, classroomId);
                    return;
                }
                else
                {
                    //create unverified user & set token & send email & create relation
                    int newUnverifiedUserId = await _userBL.CreateUnverifiedUser(email, hashedVerifyEmailToken, verifyEmailTokenCreatedDate);

                    await _emailSender.SendVerifyEmailEmail(email, newUnverifiedUserId, verifyEmailToken);
                    await _classroomRepository.CreateUserClassroomRelation(newUnverifiedUserId, classroomId);
                    return;
                }
            }
        }

        public async Task<bool> CanCallerCreateClassroomInvite(ClassroomInvite classroomInvite, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            int callerUserId = int.Parse(caller.GetUserId());
            if (callerActionAccessLevel == 8)
            {
                var classroom = await _classroomRepository.GetClassroom((int)classroomInvite.ClassroomId);
                //caller can only invite users to his own Classroom
                if (classroom.TeacherId == callerUserId)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> IsUserAStudentInClass(int classroomId, int callerUserId)
        {
            return await _classroomRepository.IsUserAStudentInClass(classroomId, callerUserId);
        }
    }
}
