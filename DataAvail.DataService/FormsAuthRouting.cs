using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataAvail.DataService
{
    public static class FormsAuthRouting
    {

        private static List<RouteAuth> _routings = new List<RouteAuth>();


        public static void MapAuth(RouteAuth[] RouteAuth)
        {
            _routings.AddRange(RouteAuth);                    
        }

        public static RequiredAuth GetAuth(string Route)
        {
            var routing = _routings.FirstOrDefault(p => Regex.IsMatch(Route, p.Route));

            if (routing.Route != null) return routing.Auth;

            routing = _routings.FirstOrDefault(p => p.Route == "*");

            if (routing.Route != null) return routing.Auth;

            return RequiredAuth.All;
        }

        public static bool NeedAuth(string Route, string Method)
        {
            var auth = GetAuth(Route);

            Method = Method.ToLower();

            if (Method == "get")
            {
                return (auth & RequiredAuth.Read) == RequiredAuth.Read;
            }
            else
            {
                return (auth & RequiredAuth.Write) == RequiredAuth.Write;
            }
        }
    }

    [Flags]
    public enum RequiredAuth
    {
        None = 0,
        All = Read | Write,
        Read =  0x1,
        Write = 0x2
    }

    public struct RouteAuth
    {
        public RouteAuth(string Route, RequiredAuth Auth)
        {
            this.Route = Route;
            this.Auth = Auth;
        }

        public string Route;

        public RequiredAuth Auth;
    }
}
