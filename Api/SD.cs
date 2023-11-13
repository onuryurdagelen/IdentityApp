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
        public const string InvalidUserNameOrPasswordMessage = "Invalid username or password";
        public const string UserNotFoundMessage = "User Not Found!";
        public const string ConfirmEmailAddressMessage = "Please confirm your email address.";
        public const string EmailAddressHasNotRegisteredYetMessage = "This email address has not been registered yet.";
        public const string InvalidEmailAddressMessage = "Invalid email address.";
        public const string InvalidTokenMessage = "Invalid token.Please try again.";
        public const string EmailAddressWasNotConfirmedYetMessage = "Your email was not confirmed yet.Please confirm your email address first!";
        public const string EmailAddressWasConfirmedBeforeMessage = "Your email was confirmed before.Please login to your account!";
        public const string FailedToSendEmailMessage = "Failed To send email.Please contact admin!";
        public const string FailedToSendForgotUserNameOrPasswordEmailMessage = "Failed To send forgot username or password email.Please contact admin!";
        public const string EmailAddressExistMessage = "An existing account is using {0}, email address. Please try with another email address!";
        public const string SuperAdminChangeNotAllowed = "Super Admin change is not allowed";
        public const string OnlySuperAdminCanSeeApplicationRoles = "Only Super Admin can see the application roles!";
        public const string AccountLockedMessage = "Your account has been locked.You should wait until {0} (UTC time) to be able to login.";
        public const string AccountLockedMessageOwingToLoginAttemps = "Your your account has been locked owing to 3 times login attempts exceeded.You should wait until {0} (UTC time) to be able to login.";

        //values
        public const int MaximumLoginAttemps = 3;

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
