using ProgrammingCloud.API.Models.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.Models
{
    public class User
    {
        public int? UserId { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FullName { get; set; }

        [Required]  //TODO: where to have validation for usertypekey?
        public string UserTypeKey { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsEmailVerified { get; set; }
        public string VerifyEmailTokenHash { get; set; }
        public DateTime? VerifyEmailTokenCreatedDate { get; set; }

        //extra
        public Dictionary<string, int> ActionAccessMappings { get; set; }
    }
}
