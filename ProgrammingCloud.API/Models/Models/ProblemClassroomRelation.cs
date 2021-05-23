using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.Models
{
    public class ProblemClassroomRelation
    {
        [Required]
        public int? ClassRoomId { get; set; }

        [Required]
        public int? ProblemId { get; set; }
    }
}
