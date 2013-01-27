using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataAvail.DataService.Provider.RequestProvider
{
    public interface IRequestProvider
    {
        string GetQueryStringParam(string ParamName);
    }
}