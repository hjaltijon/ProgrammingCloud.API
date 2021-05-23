using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Helpers
{
    public static class ExtesionMethods
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            string username = user.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(username))
            {
                throw new KeyNotFoundException("claim with caller userId not found");
            }
            return username;
        }
        public static string GetEmail(this ClaimsPrincipal user)
        {
            string username = user.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(username))
            {
                throw new KeyNotFoundException("claim with caller email not found");
            }
            return username;
        }
        public static string GetUserTypeKey(this ClaimsPrincipal caller)
        {
            string userType = caller.FindFirst("userTypeKey")?.Value;
            if (string.IsNullOrEmpty(userType))
            {
                throw new KeyNotFoundException("claim with caller UserTypeKey not found");
            }
            return userType;
        }
    }
}
