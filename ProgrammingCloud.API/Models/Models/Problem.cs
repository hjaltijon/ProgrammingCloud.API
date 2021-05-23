using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.Models
{
    public class Problem
    {
        public int ProblemId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }

        [StringLength(4000)]
        public string StudentStartingCode { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public string TestingCode { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
