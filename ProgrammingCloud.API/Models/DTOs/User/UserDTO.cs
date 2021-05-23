using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DTOs
{
    public class UserDTO
    {
        public int? UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserTypeKey { get; set; }
        public DateTime? CreatedDate { get; set; }


        //extra
        public Dictionary<string, int> ActionAccessMappings { get; set; }
    }
}
