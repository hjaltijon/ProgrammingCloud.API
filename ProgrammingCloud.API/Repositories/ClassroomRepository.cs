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
    public class ClassroomRepository
    {
        private readonly AppSettings _settings;

        public ClassroomRepository(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<int> CreateClassroom(ClassroomEntity classroomEntity)
        {
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                int classroomId = conn.QuerySingle<int>(@"
                    INSERT INTO Classrooms (Title, TeacherId)
                    OUTPUT INSERTED.ClassroomId
                    VALUES (@Title, @TeacherId)", new { Title = classroomEntity.Title, TeacherId = classroomEntity.TeacherId });
                return classroomId;
            }
        }

        public async Task<ClassroomEntity> GetClassroom(int classroomId)
        {
            string query = @"
                SELECT 
                    c.*,
	                u.FullName AS TeacherFullName
                FROM Classrooms c
                LEFT JOIN Users u ON c.TeacherId = u.UserId
                WHERE ClassroomId = @ClassroomId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                ClassroomEntity classroom = (await conn.QueryAsync<ClassroomEntity>
                    (query, new { ClassroomId = classroomId })).SingleOrDefault();
                return classroom;
            }
        }

        public async Task<List<ClassroomEntity>> GetAllClassroomsRelatedToUser(int userId)
        {
            //get classrooms where caller is the student
            string studentQuery = @"
                SELECT 
                    cr.ClassroomId,
                    cr.Title,
                    cr.CreatedDate,
                    cr.TeacherId,
                    u.FullName AS TeacherFullName
                FROM UserClassroomRelations ucrr
                INNER JOIN Classrooms cr ON ucrr.ClassroomId = cr.ClassroomId
                LEFT JOIN Users u ON cr.TeacherId = u.UserId
                WHERE ucrr.UserId = @UserId";
            //get classrooms where caller is the teacher
            string teacherQuery = @"
                SELECT 
                    cr.ClassroomId,
                    cr.Title,
                    cr.CreatedDate,
                    cr.TeacherId,
                    u.FullName AS TeacherFullName
                FROM Classrooms cr
                LEFT JOIN Users u ON cr.TeacherId = u.UserId
                WHERE cr.TeacherId = @UserId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                List<ClassroomEntity> classroomsAsStudent = (await conn.QueryAsync<ClassroomEntity>
                    (studentQuery, new { UserId = userId })).ToList();
                List<ClassroomEntity> classroomsAsTeacher = (await conn.QueryAsync<ClassroomEntity>
                    (teacherQuery, new { UserId = userId })).ToList();
                return classroomsAsStudent.Concat(classroomsAsTeacher).ToList();
            }
        }

        public async Task CreateUserClassroomRelation(int userId, int classroomId)
        {
            string insertQuery = @"
                INSERT INTO UserClassroomRelations (UserId, ClassroomId)
                VALUES (@UserId, @ClassroomId)";

            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync(insertQuery, new { UserId = userId, ClassroomId = classroomId });
            }
        }

        public async Task<bool> IsUserAStudentInClass(int classroomId, int callerUserId)
        {
            string query = "SELECT count(1) FROM UserClassroomRelations WHERE UserId = @UserId and ClassroomId = @ClassroomId";

            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                var isUserAStudentInClass = (await conn.ExecuteScalarAsync<bool>
                    (query, new { UserId = callerUserId, ClassroomId = classroomId }));
                return isUserAStudentInClass;
            }
        }
    }
}
