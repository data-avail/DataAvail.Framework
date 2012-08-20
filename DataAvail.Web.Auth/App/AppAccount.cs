using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAvail.Web.Utils;

namespace DataAvail.Web.Auth.App
{
    public class AppAccount
    {
        public AppAccount()
        { }

        public AppAccount(string Json) 
        {
            var acc = Json.FromJson<AppAccount>();

            this.AppId = acc.AppId;
            this.AppName = acc.AppName;
            this.UserName = acc.UserName;
        }

        public AppAccount(AppAccountProfile AppAccountProfile)
        {
            this.AppId = AppAccountProfile.AppId;
            this.AppName = AppAccountProfile.AppName;
            this.UserName = AppAccountProfile.RealUserName;
        }

        public int AppId { get; set; }

        public string AppName { get; set; }

        public string UserName { get; set; }
    }
}
