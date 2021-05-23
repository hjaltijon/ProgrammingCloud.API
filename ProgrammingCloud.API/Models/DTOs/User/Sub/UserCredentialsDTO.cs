using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DTOs
{
    public class UserCredentialsDTO
    {
        [Required]
        public string Password { get; set; }
    }
}
