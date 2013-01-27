using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataAvail.DataService.Provider.RequestProvider
{
    public class WebRequestProvider : IRequestProvider
    {
        public string GetQueryStringParam(string ParamName)
        {
            if (HttpContext.Current != null)
            {
                var queryString = HttpContext.Current.Request.QueryString;

                if (queryString[ParamName] != null)
                {
                    return queryString[ParamName].Trim('\'');
                }
            }

            return null;
        }
    }
}