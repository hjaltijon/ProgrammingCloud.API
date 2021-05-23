using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.Models
{
    public class UserClassroomRelation
    {
        [Required]
        public int? UserId { get; set; }

        [Required]
        public int? ClassroomId { get; set; }
    }
}
