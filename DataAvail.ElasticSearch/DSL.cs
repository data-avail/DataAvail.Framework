using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAvail.ElasticSearch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace DataAvail.ElasticSearch
{
    public class DSL
    {
        public DSL(IProxy Proxy)
        {
            this._proxy = Proxy;
        }

        private IProxy _proxy;

        private static TDest Dyn2Typed<TDest>(dynamic Dyn)
        {
            var r = JsonConvert.SerializeObject(Dyn);

            return JsonConvert.DeserializeObject<TDest>(JsonConvert.SerializeObject(Dyn));

        }

        /*
        private static dynamic GetQueryHighlight<T>()
        {

            List<string> props = new List<string>();

            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.Name != "Id" && (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(string[])))
                {
                    props.Add(prop.Name);
                }
            }

            if (props.Count != 0)
            {
                var str = string.Format("{{{0}}}",
                    string.Join(",", props.Select(p => string.Format("{0} : {{}}", p))));

                return JsonConvert.DeserializeObject<dynamic>(str);
            }
            else
            {
                return null;
            }
        }

        private string GetUndecoratedHilight(string HighlightedStr)
        {
            return HighlightedStr.Replace("<em>", "").Replace("</em>", "");
        }


        public IEnumerable<T> Query<T>(string Index, string TypeName, object Filter, bool Highlight = false)
        { 
            object hlt = null;

            if (Highlight)
            {
                hlt = GetQueryHighlight<T>();
            }

            var qRes = Query(Index, TypeName, Filter, hlt);

            return ((JArray)qRes.hits.hits).Select(p => new { id = (string)p["_id"], src = (JObject)p["_source"], ht = (JObject)p["highlight"] }).Select(p =>
            {
                if (Highlight && p.ht != null)
                {
                    foreach (var prop in p.ht)
                    {
                        if (p.src[prop.Key].Type == JTokenType.Array)
                        {
                            for (int i = 0; i < p.src[prop.Key].Count(); i++)
                            {
                                var s = ((JArray)p.src[prop.Key])[i];

                                foreach (var propVal in prop.Value.Select(x => x.Value<string>()))
                                {
                                    if (s.Value<string>() == GetUndecoratedHilight(propVal))
                                    {
                                        ((JArray)p.src[prop.Key])[i] = propVal;
                                    }
                                }
                            }
                        }
                        else if (p.src[prop.Key].Type == JTokenType.String)
                        {
                            p.src[prop.Key] = prop.Value.First.Value<string>();
                        }
                    }
                }

                var res = Dyn2Typed<T>(p.src);

                if (res != null)
                {
                    var idProp = typeof(T).GetProperty("Id");

                    if (idProp == null)
                        idProp = typeof(T).GetProperty("id");

                    if (idProp == null)
                        idProp = typeof(T).GetProperty("_id");

                    if (idProp != null)
                    {
                        idProp.SetValue(res, System.ComponentModel.TypeDescriptor.GetConverter(idProp.PropertyType).ConvertFromString(p.id), null);
                    }

                }

                return res;
            });
        }

        public dynamic Query(string Index, string TypeName, object Filter, object Highilght = null)
        {
            var filterStr = JsonConvert.SerializeObject(Filter);
           
            if (Highilght != null)
            {
                var hltStr = JsonConvert.SerializeObject(Highilght);

                if (hltStr != null)
                {
                    filterStr = filterStr.Insert(filterStr.Length-1, 
                        string.Format(", \"highlight\" : {{ \"fields\" : {0} }}", hltStr));
                }
            }

            var res = Query(Index, TypeName, filterStr);

            return JsonConvert.DeserializeObject<dynamic>(res);
        }
         */

        public string Query(string IndexName, string TypeName, string Filter)
        {
            return _proxy.Request(string.Format("{0}/{1}/_search", IndexName, TypeName), "POST", Filter);
        }

        public Dictionary<T, int> Facet<T>(string IndexName, string TypeName, object QueryObj, string FacetField, T MissedValKey)
        {
            var facetQuery = new
            {
                size = 0,
                query = QueryObj,
                facets = new {facet_name = new { terms = new { field = FacetField, size = 100} }}
            };

            var filterStr = JsonConvert.SerializeObject(facetQuery);

            var res = Query(IndexName, TypeName, filterStr);

            var jsonRes = JsonConvert.DeserializeObject<dynamic>(res);

            var dict = ((JArray)jsonRes.facets.facet_name.terms).ToDictionary(k => (T)((dynamic)k).term, v => (int)((dynamic)v).count);

            if ((int)jsonRes.facets.facet_name.missing != 0)
            {
                dict.Add(MissedValKey, (int)jsonRes.facets.facet_name.missing);
            }

            return dict;
        }

        public IEnumerable<dynamic> Query(string Index, string TypeName, dynamic QueryObject, int MaxCount = 500)
        {
            var query = new
            {
                size = MaxCount,
                query = QueryObject
            };

            var queryStr = SanitarizeQueryString(JsonConvert.SerializeObject(query));

            var res = Query(Index, TypeName, queryStr);

            var jsonRes = JsonConvert.DeserializeObject<dynamic>(res);

            return (JArray)jsonRes.hits.hits;
        }

        private static string SanitarizeQueryString(string String)
        { 
            var regex = new Regex("must_\\d");

            return regex.Replace(String, "must");
        }
    }
}
