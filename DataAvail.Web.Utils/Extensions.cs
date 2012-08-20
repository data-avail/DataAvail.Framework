using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using System.Web.Script.Serialization;
using System.Security.Principal;

namespace DataAvail.Web.Utils
{
    public static class Extensions
    {
        public static string AsJson(this object obj)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            
            return js.Serialize(obj);
        }

        public static T FromJson<T>(this string str)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            return js.Deserialize<T>(str);
        }

        public static string GetMD5Hash(this string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        public static DateTime ConvertFromUnixTimestamp(this string timestamp)
        {
            
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(long.Parse(timestamp));
        }

        public static DateTime ConvertFromUnixTimestamp(this double timestamp)
        {
            //http://codeclimber.net.nz/archive/2007/07/10/convert-a-unix-timestamp-to-a-.net-datetime.aspx
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }


        public static double ConvertToUnixTimestamp(this DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }


        public static Guid ToGuid(this string value)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));

            return new Guid(data);
        }

        public static int ToKey(this string value)
        {
            return value.ToGuid().ToKey();
        }

        public static int ToKey(this Guid value)
        {
            return int.Parse(GetMD5Hash(value.ToString()));
        }

        public static int SetAuthCookie(this HttpResponse response, string name, bool rememberMe, string UserData)
        {
            return new HttpResponseWrapper(response).SetAuthCookie(name, rememberMe, UserData);
        }

        public static int SetAuthCookie(this HttpResponseBase response, string name, bool rememberMe, string UserData)
        {
            //http://www.danharman.net/2011/07/07/storing-custom-data-in-forms-authentication-tickets/
            var cookie = FormsAuthentication.GetAuthCookie(name, rememberMe);
            var ticket = FormsAuthentication.Decrypt(cookie.Value);
            var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name,
                ticket.IssueDate, ticket.Expiration, ticket.IsPersistent,
                UserData, ticket.CookiePath);

            var encTicket = FormsAuthentication.Encrypt(newTicket);

            // Create the cookie.
            response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName,
                encTicket));

            return encTicket.Length;
        }

        public static void DeleteAuthCookie(this HttpResponse response)
        {
            FormsAuthenticationTicket ticket= new FormsAuthenticationTicket(1, "", DateTime.Now, DateTime.Now.AddMinutes(-30), false, string.Empty, FormsAuthentication.FormsCookiePath);

            response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));
        }

        public static FormsAuthenticationTicket GetFormsAuthenticationTicket(this HttpRequest request)
        {
            return request.Cookies[FormsAuthentication.FormsCookieName].GetFormsAuthenticationTicket();
        }

        public static FormsAuthenticationTicket GetFormsAuthenticationTicket(this HttpResponse response)
        {
            return response.Cookies[FormsAuthentication.FormsCookieName].GetFormsAuthenticationTicket();
        }

        public static FormsAuthenticationTicket GetFormsAuthenticationTicket(this HttpCookie FormsCookie)
        {
            if (null == FormsCookie)
                return null;

            return FormsAuthentication.Decrypt(FormsCookie.Value);
        }


        public static bool GetAuthCookie(this IIdentity Identity, out string UserName, out string UserData)
        {
            return ((FormsIdentity)Identity).Ticket.GetAuthCookie(out UserName, out UserData);
        }



        public static bool GetAuthCookie(this HttpRequest request, out string UserName, out string UserData)
        {
            return request.GetFormsAuthenticationTicket().GetAuthCookie(out UserName, out UserData);
        }

        public static bool GetAuthCookie(this FormsAuthenticationTicket ticket, out string UserName, out string UserData)
        {
            UserName = null;

            UserData = null;


            if (ticket != null)
            {
                UserData = ticket.UserData;

                UserName = ticket.Name;

                return true;
            }
            else
            {
                return false;
            }


        }



        private static string GetAuthCookieUserData(this HttpRequest request)
        {
            var ticket = request.GetFormsAuthenticationTicket();

            return ticket != null ? ticket.UserData : null;
        }

    }
}