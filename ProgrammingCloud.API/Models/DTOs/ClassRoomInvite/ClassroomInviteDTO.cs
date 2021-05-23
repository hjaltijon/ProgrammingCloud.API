using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DTOs
{
    public class ClassroomInviteDTO
    {
        public int? UserId { get; set; }
        public int? ClassroomId { get; set; }
        public DateTime? CreatedDate { get; set; }


        //joined
        public string Email { get; set; }
    }
}
