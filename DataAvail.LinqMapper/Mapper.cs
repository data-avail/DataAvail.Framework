using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DataAvail.LinqMapper
{
    public static class Mapper
    {
        private static readonly List<MappingExpression> _mappings = new List<MappingExpression>();

        public static object KeyValueIfNullForReferneceProperties = null;

        internal static MappingExpression Find<TSrc, TDest>()
        {
            return Find(typeof(TSrc), typeof(TDest));
        }

        internal static MappingExpression Find(Type SrcType, Type DestType)
        {
            return _mappings.SingleOrDefault(p => p.SrcType == SrcType && p.DestType == DestType);
        }

        public static IMappingExpression<TSrc, TDest> CreateMap<TSrc, TDest>()
        {
            var mappingExpression = new MappingExpression<TSrc, TDest>();

            var oldMapping = Find<TSrc, TDest>();

            if (oldMapping != null)
            {
                _mappings.Remove(oldMapping);
            }

            _mappings.Add(mappingExpression);

            return mappingExpression;
        }

        public static IQueryable<TDest> Map<TSrc, TDest>(IQueryable<TSrc> Src, params string [] Expands) 
        {
            var mapping = (MappingExpression<TSrc, TDest>)Find<TSrc, TDest>();

            if (mapping == null)
                throw new Exception(string.Format("Mapping between {0} and {1} not found", typeof(TSrc).Name, typeof(TDest).Name));

            var expr = (Expression<Func<TSrc, TDest>>)mapping.BuildExpression(Expands);

            return Src.Select(expr);
        }

        public static Expression<Func<TSrc, TDest>> MapExpression<TSrc, TDest>(params string[] Expands)
        {
            var mapping = (MappingExpression<TSrc, TDest>)Find<TSrc, TDest>();

            if (mapping == null)
                throw new Exception(string.Format("Mapping between {0} and {1} not found", typeof(TSrc).Name, typeof(TDest).Name));

            var expr = (Expression<Func<TSrc, TDest>>)mapping.BuildExpression(Expands);

            return expr;
        }

    }
}
