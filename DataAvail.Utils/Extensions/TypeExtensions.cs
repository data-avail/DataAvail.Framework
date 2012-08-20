using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAvail.Utils.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Get underlying type of nullable type, otherwise returns oneself
        /// </summary>
        public static Type GetCoreType(this Type Type)
        {
            return Reflection.GetCoreType(Type);
        }

        public static bool IsPrimitive(this Type Type)
        {
            return Reflection.IsPrimitive(Type);
        }

        public static T GetAttr<T>(this Type Type)
        {
            return Type.GetCustomAttributes(true).OfType<T>().FirstOrDefault();
        }

    }
}
