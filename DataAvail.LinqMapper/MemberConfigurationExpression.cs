using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace DataAvail.LinqMapper
{
    internal class MemberConfigurationExpression
    {
        internal MemberConfigurationExpression(PropertyInfo SrcPropertyInfo, PropertyInfo DestPropertyInfo)
            : this(DestPropertyInfo)
        {
            _srcPropertyInfo = SrcPropertyInfo;
        }

        internal MemberConfigurationExpression(PropertyInfo DestPropertyInfo)
        {
            _destPropertyInfo = DestPropertyInfo;
        }

        private readonly PropertyInfo _destPropertyInfo;

        private PropertyInfo _srcPropertyInfo;

        internal PropertyInfo SrcPropertyInfo
        {
            get { return _srcPropertyInfo; }
        }

        internal PropertyInfo DestPropertyInfo
        {
            get { return _destPropertyInfo; }
        }

        protected LambdaExpression ResolveExpression { get; set; }
        
        internal Expression Bind(Expression ParameterExpression)
        {
            if (ResolveExpression == null)
            {
                return TypeMap.Bind(SrcPropertyInfo, DestPropertyInfo, ParameterExpression);
            }
            else
            {
                return ResolveExpression.ConvertParam(ParameterExpression).Body;
            }
        }

        internal bool IsMapped(string[] Expands)
        {
            return ResolveExpression != null || TypeMap.IsMapped(SrcPropertyInfo, DestPropertyInfo, Expands);
        }

        public void MapFrom(PropertyInfo srcProperty)
        {
            _srcPropertyInfo = srcProperty;
        }
    }

    internal class MemberConfigurationExpression<TSrc> : MemberConfigurationExpression, IMemberConfigurationExpression<TSrc>
    {
        internal MemberConfigurationExpression(PropertyInfo DestPropertyInfo)
            : base(DestPropertyInfo)
        {
        }

        internal MemberConfigurationExpression(PropertyInfo  SrcPropertyInfo, PropertyInfo DestPropertyInfo)
            : base(SrcPropertyInfo, DestPropertyInfo)
        {
        }

        private Expression<Func<TSrc, bool>> _condition;

        #region IMemberConfigurationExpression<TSrc>

        public void MapFrom<TMember>(Expression<Func<TSrc, TMember>> sourceMember)
        {
            MapFrom((PropertyInfo)((MemberExpression)sourceMember.Body).Member);
        }

        public void Condition(Expression<Func<TSrc, bool>> condition)
        {
            _condition = condition;
        }

        public void Ignore()
        {
            base.MapFrom(null);
        }

        public void ResolveUsing<TMember>(Expression<Func<TSrc, TMember>> resolver)
        {
            ResolveExpression = resolver;
        }

        #endregion
    }
}
