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

        public static void Delete(string IndexName, string Type, string Key)
        {
            ElasticSearch es = new ElasticSearch();

            var index = es.CreateIndex();

            index.DeleteIndex(IndexName, Type, Key);
        }


        public void SetIndex<TSrc, TJson>(string IndexName, TSrc Object)
        {
            var idProperty = Object.GetType().GetProperty("Id");

            if (idProperty == null) throw new ArgumentException("Object must have public Id property"); ;

            var dest = AutoMapper.Mapper.Map<TJson>(Object);

            var key = idProperty.GetValue(Object, null);

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

        public void DeleteIndex(string IndexName, string Type, string Key)
        {
            _proxy.Request(string.Format("{0}/{1}/{2}", IndexName, Type, Key), "DELETE", string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IndexName"></param>
        /// <param name="Type"></param>
        /// <param name="Object"></param>
        /// <param name="Immediate">See http://www.elasticsearch.org/guide/reference/api/index_.html refresh option</param>
        /// <returns></returns>
        public string SetIndex(string IndexName, string Type, object Object, bool Immediate = true)
        {
            var jsonStr = JsonConvert.SerializeObject(Object);

            var res = _proxy.Request(string.Format("{0}/{1}", IndexName, Type), "POST", jsonStr);

            dynamic jsonRes = JsonConvert.DeserializeObject(res);

            if (!(bool)jsonRes.ok)
            {
                throw new Exception(res);
            }

            if (res != null)
            {
                var idProp = Object.GetType().GetProperty("Id");

                if (idProp == null)
                    idProp = Object.GetType().GetProperty("id");

                if (idProp == null)
                    idProp = Object.GetType().GetProperty("_id");

                if (idProp != null)
                {
                    idProp.SetValue(Object, (string)jsonRes._id, null);
                }
            }

            return res;
        }

        public void Flush(string IndexName)
        {
            _proxy.Request(string.Format("{0}/_flush", IndexName), "POST", null); 
        }

    }
}
