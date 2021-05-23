﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DTOs
{
    public class ProblemDTO
    {
        public int ProblemId { get; set; }
        public string Title { get; set; }
        public string StudentStartingCode { get; set; }
        public string TestingCode { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
