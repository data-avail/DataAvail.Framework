using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Profile;
using System.Security.Principal;
using DataAvail.Web.Utils;

namespace DataAvail.Web.Auth.App
{
    public class AppAccountProfile : ProfileBase
    {
        static public AppAccountProfile CreateUserProfile(string MembershipUserName, int AppId, string AppName, string RealUserName)
        {
            var profile = (AppAccountProfile)(ProfileBase.Create(MembershipUserName));

            profile.AppId = AppId;
            profile.AppName = AppName;
            profile.RealUserName = RealUserName;

            return profile;
        }

        static public AppAccountProfile GetUserProfile(string MembershipUserName)
        {
            return (AppAccountProfile)(ProfileBase.Create(MembershipUserName));
        }

        public int AppId
        {
            get { return ((int)(base["AppId"])); }

            set { base["AppId"] = value; }
        }

        public string AppName
        {
            get { return ((string)(base["AppName"])); }

            set { base["AppName"] = value; }
        }

        public string RealUserName
        {
            get { return ((string)(base["RealUserName"])); }

            set { base["RealUserName"] = value; }
        }

    }
}
