using AutoMapper;
using ProgrammingCloud.API.Helpers;
using ProgrammingCloud.API.Models.DataEntities;
using ProgrammingCloud.API.Models.DTOs;
using ProgrammingCloud.API.Models.Models;
using ProgrammingCloud.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.BusinessLogic
{
    public class ProblemBL
    {
        private readonly IMapper _mapper;
        private readonly UserBL _userBL;
        private readonly ClassroomBL _classroomBL;
        private readonly ProblemRepository _problemRepository;

        public ProblemBL(IMapper mapper,
            UserBL userBL,
            ClassroomBL classroomBL,
            ProblemRepository problemRepository)
        {
            _mapper = mapper;
            _userBL = userBL;
            _classroomBL = classroomBL;
            _problemRepository = problemRepository;
        }

        public async Task<int> CreateProblem(Problem problem)
        {
            var problemEntity = _mapper.Map<ProblemEntity>(problem);
            int problemId = await _problemRepository.CreateProblem(problemEntity);
            return problemId;
        }

        public async Task<bool> CanCallerGetAllProblemsRelatedToUser(int userId, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            int callerUserId = int.Parse(caller.GetUserId());
            if (callerUserId == userId)
            {
                return true;
            }
            return false;
        }

        public async Task<List<Problem>> GetAllProblemsRelatedToUser(int userId)
        {
            List<ProblemEntity> problemEntities = await _problemRepository.GetAllProblemsRelatedToUser(userId);
            return _mapper.Map<List<Problem>>(problemEntities);
        }

        public async Task<string> CombineStudentAndTestingCode(string studentCode, int problemId)
        {
            var problem = await GetProblem(problemId);
            string combinedCode = problem.TestingCode;
            combinedCode = combinedCode.Replace("//<[!!!THIS_COMMENT_WILL_BE_REPLACED_BY_STUDENT_CODE!!!]>", studentCode);
            return combinedCode;
        }

        public async Task<Problem> GetProblem(int problemId)
        {
            var problemEntity = await _problemRepository.GetProblem(problemId);
            return _mapper.Map<Problem>(problemEntity);
        }

        public async Task<bool> CanCallerGetProblem(Problem problem, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            if (callerActionAccessLevel == 8)
            {
                return true; //TODO: some restrictions!?
                //int callerUserId = int.Parse(caller.GetUserId());
                //if (callerUserId == problem.UserId)
                //{
                //    return true;
                //}
            }
            return false;
        }

        public async Task<bool> CanCallerUpdateProblem(Problem problem, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            if (callerActionAccessLevel == 8)
            {
                int callerUserId = int.Parse(caller.GetUserId());
                if (callerUserId == problem.UserId)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<List<CompilerError>> Compile(CompilerInfoDTO compilerInfo)
        {
            string testingCode = "";
            if (compilerInfo.UseDTOTestingCode == true)
            {
                testingCode = compilerInfo.TestingCode;
            }
            else
            {
                testingCode = (await GetProblem((int)compilerInfo.ProblemId)).TestingCode;
            }
            string[] split = testingCode.Split("//<[!!!THIS_COMMENT_WILL_BE_REPLACED_BY_STUDENT_CODE!!!]>");
            int testingCodeNumLinesFirstPart = split[0].Count(c => c.Equals('\n'));
            int studentCodeNumLines = compilerInfo.StudentCode.Count(c => c.Equals('\n')) + 1;


            string combinedCode = testingCode.Replace("//<[!!!THIS_COMMENT_WILL_BE_REPLACED_BY_STUDENT_CODE!!!]>", compilerInfo.StudentCode);
            List<string> rawErrors = CodeCompiler.Compile(combinedCode);

            var errors = new List<CompilerError>();
            foreach (var rawError in rawErrors)
            {
                if (rawError[0] == '(')
                {
                    string[] lineCol = rawError.Split(':')[0].Replace("(", "").Replace(")", "").Split(',');
                    var error = new CompilerError();
                    error.Line = int.Parse(lineCol[0]);
                    error.Column = int.Parse(lineCol[1]);
                    error.Message = rawError.Substring(lineCol[0].Length + lineCol[1].Length + 5);
                    if (error.Line > testingCodeNumLinesFirstPart && error.Line <= (testingCodeNumLinesFirstPart + studentCodeNumLines))
                    {   //Middle part is the code from the student editor, here I adjust the error location so it matches the editor
                        error.Type = "StudentEditorError";
                        error.Line = error.Line - testingCodeNumLinesFirstPart;
                    }
                    else if(error.Line > (testingCodeNumLinesFirstPart + studentCodeNumLines))
                    {   //bottom part is the code from the teacher editor, here I adjust the error location so it matches the editor
                        error.Type = "TeacherEditorError";
                        error.Line = error.Line - studentCodeNumLines + 1;
                    }
                    else
                    {   //no need to adjust the error location for the first part of the teacher editor
                        error.Type = "TeacherEditorError";
                    }
                    errors.Add(error);
                }
            }
            return errors;
        }

        public async Task UpdateProblem(Problem problem)
        {
            var problemEntity = _mapper.Map<ProblemEntity>(problem);
            await _problemRepository.UpdateProblem(problemEntity);
            return;
        }

        public async Task<bool> CanCallerCreateProblemClassroomRelation(ProblemClassroomRelation problemClassroomRelation, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            int callerUserId = int.Parse(caller.GetUserId());
            if (callerActionAccessLevel == 8)
            {
                //caller can only create relation for his own classrooms/problems
                var classroomTask = _classroomBL.GetClassroom((int)problemClassroomRelation.ClassRoomId);
                var problemTask = GetProblem((int)problemClassroomRelation.ProblemId);
                var classroom = await classroomTask;
                var problem = await problemTask;
                if (callerUserId == classroom.TeacherId && callerUserId == problem.UserId)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task CreateProblemClassroomRelation(ProblemClassroomRelation problemClassroomRelation)
        {
            var problemClassroomRelationEntity = _mapper.Map<ProblemClassroomRelationEntity>(problemClassroomRelation);
            await _problemRepository.CreateProblemClassroomRelation(problemClassroomRelationEntity);
            return;
        }

        public async Task<bool> CanCallerGetClassroomProblems(int classroomId, ClaimsPrincipal caller, int callerActionAccessLevel)
        {
            int callerUserId = int.Parse(caller.GetUserId());
            if (callerActionAccessLevel == 8)
            {
                var classroomTask = _classroomBL.GetClassroom(classroomId);
                Task<bool> isUserAStudentInClassTask = _classroomBL.IsUserAStudentInClass(classroomId, callerUserId);
                var classroom = await classroomTask;
                var isUserAStudentInClass = await isUserAStudentInClassTask;
                if (callerUserId == classroom.TeacherId || isUserAStudentInClass)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<List<Problem>> GetClassroomProblems(int classroomId)
        {
            List<ProblemEntity> problemEntities = await _problemRepository.GetClassroomProblems(classroomId);
            return _mapper.Map<List<Problem>>(problemEntities);
        }
    }
}
