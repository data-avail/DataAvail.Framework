using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace DataAvail.ElasticSearch
{
    public class Index
    {
        public Index(IProxy Proxy)
        {
            this._proxy = Proxy;
        }

        private IProxy _proxy;


        public static void Set(string IndexName, string Type, string Key, object Object)
        {
            ElasticSearch es = new ElasticSearch();

            var index = es.CreateIndex();

            index.SetIndex(IndexName, Type, Key, Object);
        }

        public void SetIndex<TSrc, TJson>(string IndexName, TSrc Object)
        {
            var idProperty = Object.GetType().GetProperty("Id");

            if (idProperty == null) throw new ArgumentException("Object must have public Id property"); ;

            var dest = AutoMapper.Mapper.Map<TJson>(Object);

            var key = idProperty.GetValue(Object);

            var type = Object.GetType().Name.ToLower();

            SetIndex(IndexName, type, key.ToString(), dest);
        }

        public void SetIndex(string IndexName, string Type, string Key, object Object)
        {
            var str = JsonConvert.SerializeObject(Object);

            var dict = new Dictionary<string, string>();

            dict.Add(Key, str);

            PutIndex(IndexName, Type, dict); 
        }

        public void PutIndex(string IndexName, string Type, IDictionary<string, string> KeyDataParams)
        {
            foreach (var kvp in KeyDataParams)
            {
                _proxy.Request(string.Format("{0}/{1}/{2}", IndexName, Type, kvp.Key), "POST", kvp.Value);
            }
        }

    }
}
