using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAvail.DataService.Provider.RequestProvider;

namespace DataAvail.DataService.Provider.Modules
{
    public class TestModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRequestProvider>().To<TestRequestProvider>().InSingletonScope();
        }
    }

}