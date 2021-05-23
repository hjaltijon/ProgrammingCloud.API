using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.Models
{
    public class Classroom
    {
        public int? ClassroomId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int TeacherId { get; set; }



        //joined
        public string TeacherFullName { get; set; }

    }
}
