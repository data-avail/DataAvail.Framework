using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAvail.Utils
{
    public class StringEx
    {
        public static string GetHtmlAttrs(object AnonymObject)
        {
            return string.Join(",", Reflection.GetAnonymProps(AnonymObject).Select(s => string.Format("{0} = '{1}'", s.Key, s.Value)));
        }

        public static string Join(string Joiner, params string [] Strs)
        {
            return string.Join(Joiner, Strs.Where(p=>!string.IsNullOrEmpty(p)));//.Remove(0, Joiner.Length);
        }
    }
}
