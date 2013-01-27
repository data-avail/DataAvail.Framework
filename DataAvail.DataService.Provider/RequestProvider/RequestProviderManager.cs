using DataAvail.DataService.Provider.Modules;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAvail.DataService.Provider.RequestProvider
{    
    public static class RequestProviderManager
    {
        public static IKernel Kernel = new StandardKernel(new WebModule());

        public static IRequestProvider RequestProvider
        {
            get
            {
                return Kernel.Get<IRequestProvider>();
            }
        }
    }
    
}
