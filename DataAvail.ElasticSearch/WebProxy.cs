using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataAvail.ElasticSearch
{
    public class WebProxy : IProxy
    {
        public WebProxy()
            : this("http://localhost:9200")
        {
        }

        public WebProxy(string BaseUrl)
        {
            this._baseUrl = BaseUrl;
        }

        private string _baseUrl;

        public string Request(string Path, string Method, string Data)
        {
            using (var client = new WebClient())
            {
                var uri = string.Format("{0}/{1}", this._baseUrl, Path);
                client.Headers[HttpRequestHeader.ContentType] = "application/xml";
                client.Encoding = System.Text.Encoding.UTF8;
                //client.Headers[HttpRequestHeader.ContentEncoding] = "utf-8";
                //client.Headers[HttpRequestHeader.AcceptCharset] = "windows-1251,utf-8;q=0.7,*;q=0.3";
                return client.UploadString(uri, Method, Data);
            }

        }
    }
}
