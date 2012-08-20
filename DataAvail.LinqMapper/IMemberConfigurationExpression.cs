using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace DataAvail.LinqMapper
{

    public interface IMemberConfigurationExpression<TSrc>
    {
        void MapFrom(PropertyInfo sorcePropery);

        void MapFrom<TMember>(Expression<Func<TSrc, TMember>> sourceMember);

        void Condition(Expression<Func<TSrc, bool>> condition);

        void Ignore();

        void ResolveUsing<TMember>(Expression<Func<TSrc, TMember>> resolver);
    }
}
