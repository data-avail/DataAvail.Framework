using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAvail.ElasticSearch
{
    public interface IProxy
    {
        string Request(string Path, string Method, string Data);
    }
}
