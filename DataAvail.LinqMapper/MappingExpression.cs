using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace DataAvail.LinqMapper
{
    internal abstract class MappingExpression
    {
        internal MappingExpression(Type SrcType, Type DestType)
        {
            this.SrcType = SrcType;

            this.DestType = DestType;
        }

        internal Type SrcType { get; private set; }

        internal Type DestType { get; private set; }

        private List<MemberConfigurationExpression> _memberConfigs = new List<MemberConfigurationExpression>();

        protected List<MemberConfigurationExpression> MemberConfigs { get { return _memberConfigs; } }

        private bool _isInitialized = false;

        protected abstract MemberConfigurationExpression OnCreateMemberConfigurationExpression(PropertyInfo SrcPropertyInfo, PropertyInfo DestPropertyInfo);

        private void EndInitialize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;

                var srcProps = SrcType.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public);

                var destProps = DestType.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public);

                var r = destProps.Where(p => Find(p) == null).Join(srcProps, d => d.Name, s => s.Name, (d, s) => new { d, s }).Where(p => TypeMap.IsValidTypePair(p.s.PropertyType, p.d.PropertyType));

                _memberConfigs.AddRange(r.Select(p => OnCreateMemberConfigurationExpression(p.s, p.d)));
            }
        }

        protected MemberConfigurationExpression Find(PropertyInfo DestPropertyInfo)
        {
            return MemberConfigs.SingleOrDefault(p => p.DestPropertyInfo == DestPropertyInfo);
        }

        #region Build Expression

        internal Expression BuildExpression(params string[] Expands)
        {
            return BuildExpression(Expression.Parameter(SrcType, "s"), Expands);
        }

        internal Expression BuildExpression(ParameterExpression ParameterExpression, string[] Expands)
        {
            return Expression.Lambda(BuildMemberInitExpression(ParameterExpression, Expands), ParameterExpression);
        }


        internal MemberInitExpression BuildMemberInitExpression(Expression ParameterExpression, string[] Expands)
        {
            EndInitialize();

            var bindings = _memberConfigs.Where(p => p.IsMapped(Expands)).Select(p => Expression.Bind(p.DestPropertyInfo, p.Bind(ParameterExpression))).ToArray();

            return Expression.MemberInit(Expression.New(DestType), bindings);
        }

        #endregion
    }

    internal class MappingExpression<TSrc, TDest> : MappingExpression, IMappingExpression<TSrc, TDest>
    {
        internal MappingExpression() : base(typeof(TSrc), typeof(TDest))
        {
        }

        protected override MemberConfigurationExpression OnCreateMemberConfigurationExpression(PropertyInfo SrcPropertyInfo, PropertyInfo DestPropertyInfo)
        {
            return new MemberConfigurationExpression<TSrc>(SrcPropertyInfo, DestPropertyInfo);
        }

        public IMappingExpression<TSrc, TDest> ForMember<TMember>(Expression<Func<TDest, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSrc>> memberOptions)
        {
            return ForMember((PropertyInfo)((MemberExpression)destinationMember.Body).Member, memberOptions);
        }

        public IMappingExpression<TSrc, TDest> ForMember(PropertyInfo DestPropertyInfo, Action<IMemberConfigurationExpression<TSrc>> memberOptions)
        {
            var memberConfig = Find(DestPropertyInfo);

            if (memberConfig == null)
            {
                memberConfig = new MemberConfigurationExpression<TSrc>(DestPropertyInfo);

                MemberConfigs.Add(memberConfig);
            }

            memberOptions((IMemberConfigurationExpression<TSrc>)memberConfig);

            return this;
        }


    }
}
