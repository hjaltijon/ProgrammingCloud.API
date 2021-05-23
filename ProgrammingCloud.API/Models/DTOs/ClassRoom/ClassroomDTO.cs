using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DTOs
{
    public class ClassroomDTO
    {
        public int ClassroomId { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int TeacherId { get; set; }


        //joined
        public string TeacherFullName { get; set; }
    }
}
