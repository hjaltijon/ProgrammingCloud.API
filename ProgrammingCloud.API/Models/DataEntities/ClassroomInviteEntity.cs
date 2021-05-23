using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DataEntities
{
    public class ClassroomInviteEntity
    {
        public int? UserId { get; set; }
        public int? ClassroomId { get; set; }
        public DateTime? CreatedDate { get; set; }


        //joined
        public string Email { get; set; }
    }
}
