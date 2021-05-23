using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DataEntities
{
    public class UserEntity
    {
        public int? UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserTypeKey { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsEmailVerified { get; set; }
        public string VerifyEmailTokenHash { get; set; }
        public DateTime? VerifyEmailTokenCreatedDate { get; set; }
    }
}
