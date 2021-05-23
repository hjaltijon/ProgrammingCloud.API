using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DTOs
{
    public class CompilerInfoDTO
    {
        public string StudentCode { get; set; }
        public string TestingCode { get; set; }
        public bool? UseDTOTestingCode { get; set; }
        public int? ProblemId { get; set; }
    }
}
