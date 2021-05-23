using Dapper;
using Microsoft.Extensions.Options;
using ProgrammingCloud.API.Configuration;
using ProgrammingCloud.API.Models.DataEntities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Repositories
{
    public class ProblemRepository
    {
        private readonly AppSettings _settings;

        public ProblemRepository(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<int> CreateProblem(ProblemEntity problemEntity)
        {
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                int classroomId = conn.QuerySingle<int>(@"
                    INSERT INTO Problems (StudentStartingCode, TestingCode, UserId, Title, Description)
                    OUTPUT INSERTED.ProblemId
                    VALUES (@StudentStartingCode, @TestingCode, @UserId, @Title, @Description)", 
                    new { StudentStartingCode = problemEntity.StudentStartingCode, TestingCode = problemEntity.TestingCode, UserId = problemEntity.UserId, Title = problemEntity.Title, Description = problemEntity.Description });
                return classroomId;
            }
        }

        public async Task<List<ProblemEntity>> GetAllProblemsRelatedToUser(int userId)
        {
            //get problems where caller is the owner
            string query = @"
                SELECT 
                    ProblemId,
                    Title,
                    CreatedDate,
                    StudentStartingCode,
                    TestingCode,
                    UserId,
                    Description
                FROM Problems
                WHERE UserId = @UserId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                List<ProblemEntity> problems = (await conn.QueryAsync<ProblemEntity>
                    (query, new { UserId = userId })).ToList();
                return problems;
            }
        }

        public async Task<ProblemEntity> GetProblem(int problemId)
        {
            string query = @"
                SELECT 
                    ProblemId,
                    Title,
                    CreatedDate,
                    StudentStartingCode,
                    TestingCode,
                    UserId,
                    Description
                FROM Problems
                WHERE ProblemId = @ProblemId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                ProblemEntity problem = (await conn.QueryAsync<ProblemEntity>
                    (query, new { ProblemId = problemId })).SingleOrDefault();
                return problem;
            }
        }

        public async Task UpdateProblem(ProblemEntity problem)
        {
            string query = @"
                UPDATE Problems
                SET StudentStartingCode = @StudentStartingCode,
                    TestingCode = @TestingCode,
                    Title = @Title,
                    Description = @Description
                WHERE ProblemId = @ProblemId
            ";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync(query, new { ProblemId = problem.ProblemId, StudentStartingCode = problem.StudentStartingCode, TestingCode = problem.TestingCode, Title = problem.Title, Description = problem.Description });
            }
        }

        public async Task CreateProblemClassroomRelation(ProblemClassroomRelationEntity problemClassroomRelationEntity)
        {
            string query = @"
                INSERT INTO ProblemClassroomRelations (ProblemId, ClassroomId)
                VALUES (@ProblemId, @ClassroomId)";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync(query, new { ProblemId = problemClassroomRelationEntity.ProblemId, ClassroomId = problemClassroomRelationEntity.ClassRoomId });
            }
        }

        public async Task<List<ProblemEntity>> GetClassroomProblems(int classroomId)
        {
            string query = @"
                SELECT 
                    p.ProblemId,
                    p.Title,
                    p.StudentStartingCode,
                    p.TestingCode,
                    p.UserId,
                    p.Description,
                    p.CreatedDate
                FROM ProblemClassroomRelations pcr
                INNER JOIN Problems p ON pcr.ProblemId = p.ProblemId
                WHERE pcr.ClassroomId = @ClassroomId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                List<ProblemEntity> problems = (await conn.QueryAsync<ProblemEntity>
                    (query, new { ClassroomId = classroomId })).ToList();
                return problems;
            }
        }
    }
}
