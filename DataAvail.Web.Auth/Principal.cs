using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace DataAvail.Web.Auth
{
    public class Principal : IPrincipal
    {
        string[] _roles;
        IIdentity _identity;

        public Principal(Identity identity, params string[] roles)
        {
            this._roles = roles;
            this._identity = identity;
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }

        public bool IsInRole(string role)
        {
            return _roles.Contains(role);

        }
    }

}