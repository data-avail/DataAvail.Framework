using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAvail.ElasticSearch
{
    public class ElasticSearch
    {
        public ElasticSearch()
            : this(new WebProxy())
        {            
        }

        public ElasticSearch(IProxy Proxy)
        {
            _proxy = Proxy;
        }

        private readonly IProxy _proxy;

        public Index CreateIndex()
        {
            return new Index(_proxy);
        }

        public DSL CreateDSL()
        {
            return new DSL(_proxy);
        }

    }
}
