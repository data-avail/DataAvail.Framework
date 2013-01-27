using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataAvail.DataService.Provider.RequestProvider
{
    public class TestRequestProvider : IRequestProvider
    {
        private readonly Dictionary<string, string> _dict = new Dictionary<string, string>();

        public string GetQueryStringParam(string ParamName)
        {
            return _dict[ParamName];
        }

        public void SetQueryStringParam(string ParamName, string Val)
        {
            _dict.Add(ParamName, Val);
        }

        public void Clear()
        {
            _dict.Clear();
        }

    }
}