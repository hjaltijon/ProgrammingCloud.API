using ProgrammingCloud.API.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Helpers
{
    public class ActionAccessMappings
    {
        private Dictionary<string, Dictionary<string, int>> _userTypeActions;

        public ActionAccessMappings()
        {
            _userTypeActions = new Dictionary<string, Dictionary<string, int>>();
            //_userTypeActions.Add("systemadmin", new Dictionary<string, int>
            //{
            //    { "GetUser", 8 },
            //    { "CreateUser", 0 }
            //});
            _userTypeActions.Add("teacher", new Dictionary<string, int>
            {
                { "CompileAndRun", 8 },
                { "Compile", 8 },
                { "GetUser", 8 },
                { "GetClassroom", 8 },
                { "GetClassrooms", 8 },
                { "CreateClassroom", 8 },
                { "CreateUserClassroomRelation", 8 },
                { "CreateProblem", 8 },
                { "UpdateProblem", 8 },
                { "GetProblems", 8 },
                { "GetProblem", 8 },
                { "GetClassroomProblems", 8 },
                { "CreateProblemClassroomRelation", 8 },
                { "GetClassroomUsers", 8 },
                
            });
            _userTypeActions.Add("student", new Dictionary<string, int>
            {
                { "CompileAndRun", 8 },
                { "Compile", 8 },
                { "GetUser", 8 },
                { "GetClassroom", 8 },
                { "GetClassrooms", 8 },
                { "CreateUserClassroomRelation", 8 },
                { "GetClassroomProblems", 8 },
                { "GetProblem", 8 }
            });
        }

        public int GetCallerActionAccessLevel(string userType, string action)
        {
            userType = userType.ToLower();
            if (_userTypeActions[userType].ContainsKey(action))
            {
                return _userTypeActions[userType][action];
            }
            return 0;
        }

        public Dictionary<string, int> GetActionAccessMappings(string userType)
        {
            userType = userType.ToLower();
            //var aamappings = new List<ActionAccessMappingDTO>();

            var userTypeActions = _userTypeActions[userType];
            //foreach (var action in userTypeActions)
            //{
            //    aamappings.Add(new ActionAccessMappingDTO
            //    {
            //        AccessLevel = action.Value,
            //        Action = action.Key
            //    });
            //}
            //return aamappings;
            return userTypeActions;
        }
    }
}
