using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Models.DataEntities
{
    public class UserClassroomRelationEntity
    {
        public int? UserId { get; set; }
        public int? ClassroomId { get; set; }
    }
}
