using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web;
using DataAvail.Web.Utils;
using System.Transactions;

namespace DataAvail.Web.Auth.App
{
    public class AppMembershipService
    {
        public static bool LogOn(string UserName, string Password, out AppAccount AppAccount)
        {
            
            if (Membership.ValidateUser(UserName, Password))
            {
                //get user 
                var user = Membership.GetUser(UserName, true);

                //get user app
                AppAccountProfile profile = AppAccountProfile.GetUserProfile(user.UserName);

                AppAccount = new AppAccount(profile);

                return true;
            }
            else
            {
                AppAccount = null;

                return false;
            }
        }

        public static MembershipUser CreateUser(string UserName, string Password, string Email, string AppName, out AppAccount AppAccount)
        {
            MembershipUser user = null;

            //using (TransactionScope txScope = new TransactionScope())
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                var userId = Guid.NewGuid();
                string userName = string.Format("{0}@{1}", UserName, AppName);
                user = Membership.CreateUser(userName, Password, Email, null, null, true, userId, out createStatus);

                if (createStatus != MembershipCreateStatus.Success)
                    throw new AppMembershipServiceException(createStatus);

                var app = CreateUserApplication(user, AppName);
                var profile = AppAccountProfile.CreateUserProfile(userName, app.Id, app.Name, UserName);
                profile.Save();
                //txScope.Complete();

                AppAccount = new AppAccount(profile);
            }

            return user;
        }

        private static UserApplication CreateUserApplication(MembershipUser MembershipUser, string AppName)
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

    }

    public class AppMembershipServiceException : System.Exception
    {
        public AppMembershipServiceException(MembershipCreateStatus MembershipCreateStatus)
        {
            this.membershipCreateStatus = MembershipCreateStatus;
        }

        public readonly MembershipCreateStatus membershipCreateStatus;
    }
}
