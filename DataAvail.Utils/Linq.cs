using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace DataAvail.Utils
{
    public static class Linq
    {
        public static IQueryable<T> ExecuteLinq<T>(string methodName, IQueryable<T> enumerable, Expression expression)
        {
            var orderByClause = expression as UnaryExpression;
            var operand = orderByClause == null ? expression as LambdaExpression : orderByClause.Operand as LambdaExpression;

            var r = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                              .Where(mi => mi.Name == methodName);

            var whereInfo = r.First();

            var currentType = enumerable.GetType();
            var seedElementType = currentType.IsArray ? currentType.GetElementType() : currentType.GetGenericArguments().ElementAt(0);

            var genericArguments = new List<Type> { seedElementType };

            if (whereInfo.GetGenericArguments().Count() > 1)
                genericArguments.Add(operand.Body.Type);

            var orderByMethod = whereInfo.MakeGenericMethod(genericArguments.ToArray());
            return (IQueryable<T>)orderByMethod.Invoke(enumerable, new object[] { enumerable, operand });
        }
    }
}
