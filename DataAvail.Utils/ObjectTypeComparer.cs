using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAvail.Utils
{
    public class ObjectTypeComparer : IEqualityComparer<object>
    {
        public ObjectTypeComparer()
        {}

        public int GetHashCode(object obj)
        {
            return obj != null ? obj.GetType().GetHashCode() : 0;
        }

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            if (x == null && y == null) return true;

            if (x == null || y == null) return false;

            return x.GetType() == y.GetType();
        }

    }
}
