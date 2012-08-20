using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DataAvail.LinqMapper
{
    //http://stackoverflow.com/questions/4601844/expression-tree-copy-or-convert

    internal class ParamSubstitutorVisitor : ExpressionVisitor
    {
        private readonly Expression newExpression;
        private readonly ParameterExpression oldParameter;

        internal ParamSubstitutorVisitor(Expression newExpression, ParameterExpression oldParameter)
        {
            this.newExpression = newExpression;
            this.oldParameter = oldParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // replace all old param references with new ones

            if (node == oldParameter) // if instance is not old parameter - do nothing
                return newExpression; //base.VisitMember(node);
            else
                return node;


            //return newExpression; // replace all old param references with new ones
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != oldParameter) // if instance is not old parameter - do nothing
                return base.VisitMember(node);

            var newObj = Visit(node.Expression);
            var newMember = newExpression.Type.GetMember(node.Member.Name).First();
            return Expression.MakeMemberAccess(newObj, newMember);


        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node != null && node.Method.DeclaringType == typeof(DataAvail.LinqMapper.Mapper) && node.Method.Name == "MapExpression")
            {
                return (LambdaExpression) node.Method.Invoke(null, new object[] { new string[0] });
            }
            else
            {
                var methodCall = base.VisitMethodCall(node);

                return methodCall;
            }
        }
    }

    public static class LinqExtentions
    {
        public static LambdaExpression ConvertParam(this LambdaExpression expression, Expression newExpression)
        {
            var oldParameter = expression.Parameters[0];
            var converter = new ParamSubstitutorVisitor(newExpression, oldParameter);
            var newBody = converter.Visit(expression.Body);
            return Expression.Lambda(newBody, expression.Parameters[0]);
        }

    }

}
