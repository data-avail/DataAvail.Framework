using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace DataAvail.LinqMapper
{
    public interface IMappingExpression<TSrc, TDest>
    {
        IMappingExpression<TSrc, TDest> ForMember(PropertyInfo PropertyInfo, Action<IMemberConfigurationExpression<TSrc>> memberOptions);
        IMappingExpression<TSrc, TDest> ForMember<TMember>(Expression<Func<TDest, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSrc>> memberOptions);
    }

}
