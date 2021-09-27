
using EPP.CorporatePortal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static EPP.CorporatePortal.Common.Enums;

namespace EPP.CorporatePortal.Helper
{
    public static class AuthorizationHelper
    {
        /// <summary>  
        /// Determines whether the specified user permissions has requested permission.  
        /// </summary>  
        /// <param name="userPermissions">The user's permissions.</param>  
        /// <param name="requestedPermissions">The expected permissions.</param>  
        /// <param name="userName">Name of the user.</param>  
        /// <param name="comparisonType">Type of the comparison.</param>  
        /// <returns>  
        /// True, If requested permissions exists in User's permission list  
        /// </returns>  
        internal static bool HasPermission(IEnumerable<Rights_Enum> userPermissions, IEnumerable< Rights_Enum> requestedPermissions, ComparisonType comparisonType = ComparisonType.All)
        { 
            if (requestedPermissions == null || userPermissions == null)
            {
                return false;
            }
            bool hasPermission = false;
            switch (comparisonType)
            {
                case ComparisonType.All:
                    hasPermission = requestedPermissions.All(reqPerm => userPermissions.Any(usrPerm => usrPerm == reqPerm));
                    break;
                case ComparisonType.Any:
                    hasPermission = requestedPermissions.Any(reqPerm => userPermissions.Any(usrPerm => usrPerm == reqPerm));
                    break;
                default:
                    throw new ArgumentException("New comparison type need to be included");
            }
            return hasPermission;
        }

    }
}