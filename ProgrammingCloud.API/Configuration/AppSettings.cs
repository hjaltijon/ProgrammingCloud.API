using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Configuration
{
    public class AppSettings
    {
        public string SqlConnectionString { get; set; }
        public string EmailPassword { get; set; }
        public string HmacSecret { get; set; }
        public string FunctionSecret { get; set; }


        //jason web token stuff:
        public string JwtIssuer { get; set; }
        public string JwtKey { get; set; }
    }
}

