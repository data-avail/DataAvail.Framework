using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using DataAvail.Web.Utils;

namespace DataAvail.Web.Auth.App
{
    public class AppIdentity : Identity
    {
        public AppIdentity(IIdentity Identity) : base(Identity.Name)
        {
            string userName = null;

            string userData = null;

            if (Identity.GetAuthCookie(out userName, out userData))
            {
                this.AppAccount = new AppAccount(userData); 
            }
        }

        public AppAccount AppAccount { get; private set; }

    }
}
