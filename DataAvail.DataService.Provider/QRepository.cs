using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.Data.Services.Toolkit.QueryModel;
using DataAvail.LinqMapper;
using System.Data.Services.Common;

namespace DataAvail.DataService.Provider
{
    public abstract class QRepository<E, T> 
    {
        protected abstract IQueryable<E> Queryable {get;}

        protected virtual int PageSize { get { return 20; } }

        protected static string[] GetExpands()
        {
            var expand = DataServiceProvider.GetQueryStringParam("$expand");

            return expand != null ? expand.Split(',') : new string[0];

        }

        /*
        public static dynamic GetCustomFilter()
        {
            var customFilter = GetQueryStringParam("customFilter");

            if (customFilter != null)
            {
                //TO DO : temp fix
                customFilter = customFilter.Replace("$","_");

                JsonFx.Serialization.DataReaderSettings sgs = new JsonFx.Serialization.DataReaderSettings();

                var jsonReader = new JsonFx.Json.JsonReader(sgs);

                return jsonReader.Read(customFilter);
            }
            else
            {
                return null;
            }
        }


        protected static string GetQueryStringParam(string ParamName)
        {
            if (HttpContext.Current != null)
            {
                var queryString = HttpContext.Current.Request.QueryString;

                if (queryString[ParamName] != null)
                {
                    return queryString[ParamName];
                }
            }

            return null;        
        }
         */

        public virtual T GetOne(string id)
        {
            /*
            var q = Mapper.Map<E, T>(GetQueryableForGetOne(), GetExpands());

            q = FilterBy<T>(q, KeyFieldName, ParseKey(id));

            return q.FirstOrDefault();
             */

            var q = GetQueryableForGetOne();

            q = FilterBy(q, EntityKeyFieldName, ParseKey(id));

            var q1 = Mapper.Map<E, T>(q, GetExpands());

            return q1.FirstOrDefault();
        }

        protected virtual object ParseKey(string key)
        {
            var pi = (PropertyInfo)(typeof(E)).GetMember(EntityKeyFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];

            if (pi.PropertyType == typeof(int))
            {
                return int.Parse(key);
            }
            else
            {
                return key;
            }

        }

        public virtual IQueryable<E> GetQueryableForGetAll()
        {
            return Queryable;
        }

        public virtual IQueryable<E> GetQueryableForGetOne()
        {
            return Queryable;
        }

        public virtual IQueryable<T> GetAll(ODataQueryOperation operation)
        {
            var q = Mapper.Map<E, T>(GetQueryableForGetAll(), GetExpands());

            if (operation.FilterExpression != null)
            {
                var expr = (System.Linq.Expressions.Expression<System.Func<T, bool>>)((UnaryExpression)operation.FilterExpression).Operand;

                q = q.Where(expr);
            }

            if (operation.TopCount == 0)
                operation.TopCount = PageSize;

            if (operation.SkipCount > 0)
            {
                q = q.Skip(operation.SkipCount);
            }


            while (operation.OrderStack != null && operation.OrderStack.Count > 0)
            {
                var orderExpression = operation.OrderStack.Pop();
                q = Utils.Linq.ExecuteLinq(orderExpression.OrderMethodName, q, orderExpression.Expression);
            }

            if (operation.TopCount > 0)
            {
                q = q.Take(operation.TopCount);
            }

            operation.OrderStack = null;
            operation.SkipCount = 0;
            operation.TopCount = 0;

            return q;
        }

        /*
        private IQueryable<T> FilterByKey(IQueryable<T> Queryable, string KeyValue)
        {
            return FilterBy(Queryable, KeyFieldName, KeyValue);
        }

        private object ConvertKey(string Key, PropertyInfo KeyField)
        {
            if (KeyField.PropertyType == typeof(int))
            {
                return int.Parse(Key);
            }
            else
            {
                return Key;
            }
        }

        private IQueryable<T> FilterBy(IQueryable<T> Queryable, string FieldName, string Value)
        {
            var type = typeof(T);

            //just first huh?
            var keyField = (PropertyInfo)type.GetMember(FieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];

            object key = ConvertKey(Value, keyField);

            ParameterExpression prm = Expression.Parameter(typeof(T), "x");
            MemberExpression member = Expression.MakeMemberAccess(prm, keyField);
            ConstantExpression idParam = Expression.Constant(key, key.GetType());
            BinaryExpression expr = Expression.Equal(member, idParam);
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(expr, new ParameterExpression[] { prm });

            return Queryable.Where(lambda);
        }
         */

        private IQueryable<X> FilterBy<X>(IQueryable<X> Queryable, string FieldName, object Value)
        {
            var type = typeof(X);
            var pi = (PropertyInfo)type.GetMember(FieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];
            ParameterExpression prm = Expression.Parameter(typeof(X), "x");
            MemberExpression member = Expression.MakeMemberAccess(prm, pi);
            ConstantExpression idParam = Expression.Constant(Value, Value.GetType());
            BinaryExpression expr = Expression.Equal(member, idParam);
            Expression<Func<X, bool>> lambda = Expression.Lambda<Func<X, bool>>(expr, new ParameterExpression[] { prm });

            return Queryable.Where(lambda);
        }


        protected virtual string EntityKeyFieldName
        {
            get
            {
                //TO DO
                return "Id";
            }
        }


        protected string KeyFieldName
        {
            get
            {
                var type = typeof(T);

                return type.GetCustomAttributes(true).OfType<DataServiceKeyAttribute>().SingleOrDefault().KeyNames[0];
            }
        }

    }
}
