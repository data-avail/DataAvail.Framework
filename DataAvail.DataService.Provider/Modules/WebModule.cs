using Ninject.Modules;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAvail.DataService.Provider.RequestProvider;

namespace DataAvail.DataService.Provider.Modules
{
    public class WebModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRequestProvider>().To<WebRequestProvider>().InSingletonScope();
        }
    }
}