using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DataAvail.Web.Utils;
using System.Web.Security;

namespace DataAvail.Web.Auth.App
{
    public static class MembershipUserExtensions
    {
        public static UserApplication CreateUserApplication(this MembershipUser MembershipUser, string AppName)
        {
            using (var context = new App.AccountEntities(/*"name=DefaultConnection"*/))
            {
                var userApplication = new UserApplication
                {
                    Name = AppName
                };

                var user = context.Users.FirstOrDefault(p => p.UserId == (Guid)MembershipUser.ProviderUserKey);

                user.UserApplication = userApplication;

                context.SaveChanges();

                return userApplication;
            }
        }

     
        /*
        public static bool LogOn(this HttpRequest HttpRequest, out string Name, out string UserData)
        {
            Name = null;

            UserData = null;

            string authHeader = HttpRequest.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authHeader))
            {
                var credentials = Utils.ParseAuthHeader(authHeader);

                if (AppMembershipService.LogOn(credentials[0], credentials[1], out UserData))
                {
                    Name = credentials[0];

                    return true;
                }
            }

            return false;
        }


        public static string GetAppName(this HttpContext HttpContext)
        {
            if (HttpContext.User != null)
            {
                if (HttpContext.User is IPrincipalUserData)
                {
                    return ((IPrincipalUserData)HttpContext.User).UserData;
                }
                else
                {
                    var ticket = HttpContext.Request.GetFormsAuthenticationTicket();

                    return ticket.UserData;
                }
            }
            else
            {
                return null;
            }
        }
         */
    }
}
