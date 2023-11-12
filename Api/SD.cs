using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Api
{
    //static details
    public static class SD
    {
        public const string Facebook = "facebook";
        public const string Google = "google";

        //Roles
        public const string SuperAdminRole = "super_admin";
        public const string AdminRole = "admin";
        public const string ManagerRole = "manager";
        public const string PlayerRole = "player";

        //Policies
        public const string SuperAdminPolicy = "super_admin_required";
        public const string AdminPolicy = "admin_required";
        public const string ManagerPolicy = "manager_required";
        public const string PlayerPolicy = "player_required";
        public const string AdminOrManagerPolicy = "admin_or_manager_required";
        public const string AdminAndManagerPolicy = "admin_and_manager_required";
        public const string AllRolesPolicy = "all_roles_required";
        public const string AdminEmailPolicy = "admin_email_required";

        //claim policies
        public const string SuperAdminEmailAddress = "superadmin@example.com";
        public const string AdminEmailAddress = "admin@example.com";
        public const string PaulsonSurnamePolicy = "PaulsonSurnamePolicy";
        public const string PaulsonSurname = "Paulson";
        public const string ManagerEmailAddress = "manager@example.com";
        public const string ManagerEmailAndPaulsonSurnamePolicy = "ManagerEmailAndPulsonSurnamePolicy";
        public const string VipPolicy = "VIPPolicy";

        //usernames
        public const string SuperAdminUserName = "admin@example.com";
        public const string AdminUserName = "admin@example.com";
        public const string ManagerUserName = "admin@example.com";
        public const string PlayerUserName = "admin@example.com";

        //messages
        public const string SuperAdminChangeNotAllowed = "Super Admin change is not allowed";
        public const string OnlySuperAdminCanSeeApplicationRoles = "Only Super Admin can see the application roles!";

        public static bool VIPPolicy(AuthorizationHandlerContext context)
        {
            if(context.User.IsInRole(PlayerRole) &&
                context.User.HasClaim(c => c.Type == ClaimTypes.Email && c.Value.Contains("vip")))
            {
                return true;
            }
            return false;
        }
    }
}
