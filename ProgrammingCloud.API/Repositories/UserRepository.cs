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
    public class UserRepository
    {
        private readonly AppSettings _settings;

        public UserRepository(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }
        
        public async Task<UserEntity> GetUserByEmail(string email)
        {
            string query = "select * from Users where Email = @Email AND IsEmailVerified = 1";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                UserEntity user = (await conn.QueryAsync<UserEntity>
                    (query, new { Email = email })).SingleOrDefault();
                return user;
            }
        }
        public async Task<UserEntity> GetUserWithUnVerifiedEmail(string email)
        {
            string query = "select * from Users where Email = @Email AND IsEmailVerified = 0";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                UserEntity user = (await conn.QueryAsync<UserEntity>
                    (query, new { Email = email })).SingleOrDefault();
                return user;
            }
        }

        public async Task<UserEntity> GetUserWithUnVerifiedEmail(int userId)
        {
            string query = "select * from Users where UserId = @UserId AND IsEmailVerified = 0";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                UserEntity user = (await conn.QueryAsync<UserEntity>
                    (query, new { UserId = userId })).SingleOrDefault();
                return user;
            }
        }
        public async Task<UserEntity> GetUserByUserId(int userId)
        {
            string query = "select * from Users where UserId = @UserId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                UserEntity user = (await conn.QueryAsync<UserEntity>
                    (query, new { UserId = userId })).SingleOrDefault();
                return user;
            }
        }

        public async Task RegisterVerifyEmailToken(int userId, string hashedVerifyEmailToken, DateTime verifyEmailTokenCreatedDate)
        {
            string query = @"
                UPDATE Users
                SET VerifyEmailTokenHash = @VerifyEmailTokenHash, VerifyEmailTokenCreatedDate = @VerifyEmailTokenCreatedDate
                WHERE UserId = @UserId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync(query, new { UserId = userId, VerifyEmailTokenHash = hashedVerifyEmailToken, VerifyEmailTokenCreatedDate = verifyEmailTokenCreatedDate });
            }
        }

        public async Task<int> CreateUnverifiedUser(string email, string hashedVerifyEmailToken, DateTime verifyEmailTokenCreatedDate)
        {
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                int userId = conn.QuerySingle<int>(@"
                    INSERT INTO Users (Email, IsEmailVerified, VerifyEmailTokenHash, VerifyEmailTokenCreatedDate)
                    OUTPUT INSERTED.UserId
                    VALUES (@Email, 0, @VerifyEmailTokenHash, @VerifyEmailTokenCreatedDate)", new { Email = email, VerifyEmailTokenHash = hashedVerifyEmailToken, VerifyEmailTokenCreatedDate = verifyEmailTokenCreatedDate });
                return userId;
            }
        }

        public async Task<List<UserEntity>> GetClassroomUsers(int classroomId)
        {
            string query = @"
                SELECT 
	                u.*
                FROM UserClassroomRelations ucr
                INNER JOIN Users u ON ucr.UserId = u.UserID
                WHERE ucr.ClassroomId = @ClassroomId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                List<UserEntity> users = (await conn.QueryAsync<UserEntity>
                    (query, new { ClassroomId = classroomId })).ToList();
                return users;
            }
            
        }

        public async Task ActivateUser(UserEntity user)
        {
            string query = @"
                UPDATE Users
                SET FullName = @FullName, 
                    UserTypeKey = @UserTypeKey,
                    PasswordHash = @PasswordHash,
                    Salt = @Salt,
                    IsEmailVerified = @IsEmailVerified,
                    VerifyEmailTokenHash = @VerifyEmailTokenHash,
                    VerifyEmailTokenCreatedDate = @VerifyEmailTokenCreatedDate
                WHERE UserId = @UserId";
            using (SqlConnection conn = new SqlConnection(_settings.SqlConnectionString))
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync(query, new {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    UserTypeKey = user.UserTypeKey,
                    PasswordHash = user.PasswordHash,
                    Salt = user.Salt,
                    IsEmailVerified = user.IsEmailVerified,
                    VerifyEmailTokenHash = user.VerifyEmailTokenHash,
                    VerifyEmailTokenCreatedDate = user.VerifyEmailTokenCreatedDate
                });
            }
        }
    }
}
