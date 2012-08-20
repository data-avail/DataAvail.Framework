using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAvail.Web.Auth
{
    public static class Utils
    {
        public static string[] ParseAuthHeader(string authHeader)
        {
            // Check this is a Basic Auth header 
            if (
                authHeader == null ||
                authHeader.Length == 0 ||
                !authHeader.StartsWith("Basic")
            ) return null;

            // Pull out the Credentials with are seperated by ':' and Base64 encoded 
            string base64Credentials = authHeader.Substring(6);
            string[] credentials = Encoding.ASCII.GetString(
                  Convert.FromBase64String(base64Credentials)
            ).Split(new char[] { ':' });

            if (credentials.Length != 2 ||
                string.IsNullOrEmpty(credentials[0]) ||
                string.IsNullOrEmpty(credentials[0])
            ) return null;

            // Okay this is the credentials 
            return credentials;
        }

    }
}
