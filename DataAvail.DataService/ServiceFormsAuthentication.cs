using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DataAvail.Web.Auth;
using System.Web.Security;
using System.Security.Principal;
using System.Threading;
using DataAvail.Web.Utils;

namespace DataAvail.DataService
{
    public class ServiceFormsAuthentication
    {
        public static void AuthenticateRequest(Func<string, string> OnGetUserData = null)
        {
            var httpContext = HttpContext.Current;

            if (!httpContext.Request.IsAuthenticated)
            {
                string authHeader = httpContext.Request.Headers["Authorization"];

                if (!string.IsNullOrEmpty(authHeader))
                {
                    var credentials = Utils.ParseAuthHeader(authHeader);

                    if (Membership.ValidateUser(credentials[0], credentials[1]))
                    {
                        if (OnGetUserData != null)
                        {
                            var userData = OnGetUserData(credentials[0]);

                            httpContext.Response.SetAuthCookie(credentials[0], true, userData);
                        }

                        httpContext.User = new GenericPrincipal(new FormsIdentity(httpContext.Response.GetFormsAuthenticationTicket()), 
                            Roles.Enabled ? Roles.GetRolesForUser(credentials[0]) : new string [] {});

                        return;
                    }
                }

                RequestAuthentication();
            }

        }

        public static void PostAuthenticateRequest(Func<IPrincipal, IPrincipal> PrincipalModifyer = null)
        {
            var httpContext = HttpContext.Current;

            if (httpContext.Request.IsAuthenticated)
            {
                if (PrincipalModifyer != null)
                {
                    var newPrincipal = PrincipalModifyer(httpContext.User);

                    httpContext.User = newPrincipal;

                    Thread.CurrentPrincipal = newPrincipal;
                }
            }
        }

        public static void RequestAuthentication()
        {
            var httpContext = HttpContext.Current;

            //Require base auth parameters 
            httpContext.Response.Status = "401 Unauthorized";
            httpContext.Response.StatusCode = 401;
            if (httpContext.Request.Headers["Origin"] == null)
                httpContext.Response.AddHeader("WWW-Authenticate", "Basic");
            httpContext.Response.End();
        }

        public static void CheckRequireSsl()
        {
            var httpContext = HttpContext.Current;

            if (FormsAuthentication.RequireSSL && !httpContext.Request.IsSecureConnection)
            {
                httpContext.Response.Status = "403 Forbidden";
                httpContext.Response.StatusCode = 403;
                httpContext.Response.SubStatusCode = 4;
                httpContext.Response.End();
            }

        }


        public static void LogOut()
        {
            var context = HttpContext.Current;
            context.Session.Clear();
            context.Session.Abandon();
            context.Response.DeleteAuthCookie();

            //remove .ASPX cookies
            FormsAuthentication.SignOut();
            ServiceFormsAuthentication.RequestAuthentication();
        }
    }
}
