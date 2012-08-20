using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using DataAvail.Web.Utils;

namespace DataAvail.Web.Auth.OAuth
{
    internal static class OAuthExtensions
    {

        internal static void SetUserIdentifyer(this HttpResponse response, OAuthUserIdentifyer UserIdentifyer)
        {
            response.SetAuthCookie(UserIdentifyer.UserName, true, UserIdentifyer.ServiceIdentifyer.AsJson());
        }

        internal static OAuthUserIdentifyer GetUserIdentifyer(this HttpRequest request)
        {
            var ticket = request.GetFormsAuthenticationTicket();

            if (ticket != null)
            {
                var userIdent = new OAuthUserIdentifyer() { UserName = ticket.Name };

                if (!string.IsNullOrEmpty(ticket.UserData))
                {
                    try
                    {
                        userIdent.ServiceIdentifyer = ticket.UserData.Split('|')[0].FromJson<OAuthServiceIdentifyer>();
                    }
                    catch (Exception e){
                        throw new Exception("Looks like service identity data in user authetication cookie in the wrong format", e);
                    }
                }

                return userIdent;
            }
            else
            {
                return null;
            }
        }

    }
}