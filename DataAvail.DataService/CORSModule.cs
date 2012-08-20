using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DataAvail.DataService
{
    public class CORSModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            if (!AllowAccessPolicy.FullfillPolicy("*", true, app.Context.Request.Headers["Origin"] ?? app.Context.Request.Url.Authority, app.Context.Response.Headers))
            {
                app.Response.End();
            }
        }

        public void Dispose()
        {
            
        }
    }
}
