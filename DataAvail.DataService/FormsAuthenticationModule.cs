using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using DataAvail.Web.Auth.App;
using DataAvail.Web.Auth;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Web;
using DataAvail.Web.Utils;

namespace DataAvail.DataService
{
    public class FormsAuthenticationModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest
               += new EventHandler(context_AuthenticateRequest);

            context.PostAuthenticateRequest 
                += new EventHandler(context_PostAuthenticateRequest);

            context.EndRequest 
                += new EventHandler(context_EndRequest);
        }

        void context_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (FormsAuthRouting.NeedAuth(app.Request.Path, app.Request.HttpMethod))
            {
                ServiceFormsAuthentication.CheckRequireSsl();
                ServiceFormsAuthentication.AuthenticateRequest(p => new AppAccount(AppAccountProfile.GetUserProfile(p)).AsJson());
            }
        }

        void context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            ServiceFormsAuthentication.PostAuthenticateRequest(p => new Principal(new AppIdentity(p.Identity)));
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            //1.I'm said - I'm unauthenticated (In AuthenticateRequest)
            //2.Forms authentication - Ok then redirect to login URL
            //3.And then I'm here again, no - I better know what to do, don't redirect just return Unauthenticated response
            //Same for require SSL
            HttpApplication app = (HttpApplication)sender;
            if (app.Response.RedirectLocation == null && app.Response.StatusCode == 302)
            {
                //app.Response.ClearHeaders();
                app.Response.ClearContent();
                ServiceFormsAuthentication.CheckRequireSsl();
                ServiceFormsAuthentication.RequestAuthentication();
            }
        }

        public void Dispose() { }

    }


}
