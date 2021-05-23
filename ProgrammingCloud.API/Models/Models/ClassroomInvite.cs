using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.Models
{
    public class ClassroomInvite
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Email { get; set; }

        public int? UserId { get; set; }

        [Required]
        public int? ClassroomId { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
