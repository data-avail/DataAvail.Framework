using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace DataAvail.Web.Auth
{
    public class Identity : IIdentity
    {
        string _name;

        public Identity(string name)
        {
            this._name = name;
        }

        string IIdentity.AuthenticationType
        {
            get { return "DataAvail.DataHub.Service SCHEME"; }
        }

        bool IIdentity.IsAuthenticated
        {
            get { return true; }
        }

        string IIdentity.Name
        {
            get { return _name; }
        }
    }
}