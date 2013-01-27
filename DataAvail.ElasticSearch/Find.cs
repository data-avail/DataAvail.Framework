using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAvail.ElasticSearch
{
    public static class Find
    {
        /*
        public static IEnumerable<T> Like<T>(string Index, string Type, string Term, bool Highlight = true)
        {
            DataAvail.ElasticSearch.ElasticSearch es = new DataAvail.ElasticSearch.ElasticSearch();

            var dsl = es.CreateDSL();

            var search = new
            {
                query =
                    new
                    {
                        @bool =
                            new
                            {
                                must =
                                    new[] { new { query_string = new { default_field = "_all", query = string.Format("*{0}*", Term) } } }
                            }
                    }
            };

            return dsl.Query<T>(Index, Type, search, Highlight);
            
        }

        public static IEnumerable<T> Likes<T>(string Index, string Type, string [] Term, bool Highlight = true)
        {
            DataAvail.ElasticSearch.ElasticSearch es = new DataAvail.ElasticSearch.ElasticSearch();

            var dsl = es.CreateDSL();

            string queryTerm = string.Join(" AND ", Term.Select(p=>string.Format("*{0}*", p)));

            var search = new
            {
                query =
                    new
                    {
                        @bool =
                            new
                            {
                                must =
                                    new[] { new { query_string = new { default_field = "_all", query = queryTerm } } },
                            }
                    }
            };

            return dsl.Query<T>(Index, Type, search, Highlight);

        }


        //
        public static IEnumerable<T> Likes<T>(string Index, string Type, string [] Fields, string [] Term, bool Highlight = true)
        {
            DataAvail.ElasticSearch.ElasticSearch es = new DataAvail.ElasticSearch.ElasticSearch();

            var dsl = es.CreateDSL();

            string queryTerm = string.Join(" AND ", Term.Select(p => string.Format("(*{0}* OR {0}~0.7)", p)));

            var search = new
            {
                query =
                    new
                    {
                        @bool =
                            new
                            {
                                should =
                                    new [] { new { query_string = new { fields = Fields, query = queryTerm } } }
                            }
                    }
            };

            return dsl.Query<T>(Index, Type, search, Highlight);

        }


        public static IEnumerable<T> Fuzzy<T>(string Index, string Type, string[] Fields, string[] Term, bool Highlight = true)
        {
            DataAvail.ElasticSearch.ElasticSearch es = new DataAvail.ElasticSearch.ElasticSearch();

            var dsl = es.CreateDSL();

            string queryTerm = string.Join(" AND ", Term.Select(p => string.Format("*{0}*", p)));

            var search = new
            {
                query = new
                {
                    fuzzy = new { _all = queryTerm }
                }
            };

            return dsl.Query<T>(Index, Type, search, Highlight);

        }
         */

    }
}
