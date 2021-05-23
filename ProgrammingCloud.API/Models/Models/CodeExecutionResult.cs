using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.Models
{
    public class CodeExecutionResult
    {
        public List<Test> Tests { get; set; }

        public bool? EcounteredCompilerErrors { get; set; }
        public List<Error> CompilerErrors { get; set; }
    }

    public class Error
    {
        public string Location { get; set; }
        public string Message { get; set; }
    }

    public class Test
    {
        public string Name { get; set; }
        public bool Success { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public string Input { get; set; }
        public string Message { get; set; }
    }
}
