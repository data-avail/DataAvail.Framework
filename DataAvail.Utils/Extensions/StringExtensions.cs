using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace DataAvail.Utils.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replace charecter in the string by using Replacing.
        /// Replacing should return '\r' for empy character
        /// </summary>
        public static string Replace(this string Str, Func<char, bool> KeepChar)
        {
            return new string(Str.Select((c) =>
            {
                return KeepChar(c) ? c : '\r';
            }).ToArray()).Replace("\r", "");
        }

        public static IEnumerable<string> Trim(this IEnumerable<string> Strs, params string [] trim)
        {
            return Strs.Select(p=>p.Trim(trim.SelectMany(s=>s.ToCharArray()).ToArray()));
        }

        public static string Couple(this IEnumerable<string> Strs, params string [] Prefixes)
        {
            return string.Join("", Strs.Select((p, i) => string.IsNullOrEmpty(p) ? "" : string.Format("{0}{1}", Prefixes.ElementAt(i), p)));
        }

        public static object ConvertToType(this string String, Type ConvertType)
        {
            return ConvertToType(String, ConvertType, true);
        }

        public static object ConvertToType(this string String, Type ConvertType, bool IsEmptyNull)
        {
            if (String.IsNullOrEmpty(String))
                return IsEmptyNull ? null : System.DBNull.Value;

            return System.Convert.ChangeType(String, Reflection.GetCoreType(ConvertType));
        }

        public static string ConstructQueryString(this NameValueCollection NameValueCollection)
        {
            List<String> items = new List<String>();

            foreach (String name in NameValueCollection)
                items.Add(String.Concat(name, "=", NameValueCollection[name]));

            return String.Join("&", items.ToArray());

        }

        public static string ListToString(this IEnumerable<string> StrList)
        {
            return ListToString(StrList, ",");
        }

        public static string ListToString(this IEnumerable<string> StrList, string Separ)
        {
            return String.Join(Separ, StrList.ToArray());
        }

        public static string ListToString<T>(this IEnumerable<T> List, Func<T, string> FieldAccess, string Separ)
        {
            return ListToString(List.Select(FieldAccess), Separ);
        }

        public static string ListToString<T>(this IEnumerable<T> List, Func<T, string> FieldAccess, string Separ, string Foramtter)
        {
            Func<T, string> fa = p => string.Format(Foramtter, FieldAccess(p));

            return ListToString(List, fa, Separ);
        }

        public static bool IsTag(this string Str)
        {
            return Str.Trim().StartsWith("<") && Str.Trim().EndsWith(">");
        }

        public static string WrapTag(this string Str, string TagName, object HtmlAttrs)
        {
            return string.Format("<{0} {1}>{2}</{0}>", TagName, StringEx.GetHtmlAttrs(HtmlAttrs), Str);
        }

        public static IEnumerable<string> ExtractUrls(this string Str)
        {
            //from regexlib.com
            Regex UrlMatcher = new Regex(@"([\d\w-.]+?\.(a[cdefgilmnoqrstuwz]|b
		                                [abdefghijmnorstvwyz]|c[acdfghiklmnoruvxyz]|d[ejkmnoz]|e
		                                [ceghrst]|f[ijkmnor]|g[abdefghilmnpqrstuwy]|h[kmnrtu]|i
		                                [delmnoqrst]|j[emop]|k[eghimnprwyz]|l[abcikrstuvy]|m
		                                [acdghklmnopqrstuvwxyz]|n[acefgilopruz]|om|p[aefghklmnrstwy]|
		                                qa|r[eouw]|s[abcdeghijklmnortuvyz]|t[cdfghjkmnoprtvwz]|
		                                u[augkmsyz]|v[aceginu]|w[fs]|y[etu]|z[amw]|aero|arpa|biz|
		                                com|coop|edu|info|int|gov|mil|museum|name|net|org|pro)
		                                (\b|\W(?<!&|=)(?!\.\s|\.{3}).*?))(\s|$)");

            return UrlMatcher.Matches(Str).Cast<Match>().Select(p => p.Value);
        }

        public static IEnumerable<string> GetBetweenMany(this string Str, string Left, IEnumerable<string> Rights)
        {
            return Rights.SelectMany(p=>Str.GetBetweenMany(Left, p)).Where(p=> p != null);
        }

        public static IEnumerable<string> GetBetweenMany(this string Str, string Left, string Right)
        {
            var i = 0;

            while (i != -1)
            {
                var s = GetBetween(Str, Left, Right, i);

                if (!string.IsNullOrEmpty(s))
                {
                    i = Str.IndexOf(s);

                    if (i == -1)
                        throw new Exception("Something wrong");
                 
                    yield return s;
                }
                else
                    i = -1;
            }
        }

        public static string GetBetween(this string Str, string Left, string Right)
        {
            return GetBetween(Str, Left, Right, 0);
        }

        public static string GetBetween(this string Str, string Left, string Right, int StartsIndex)
        {
            var i = Str.IndexOf(Left, StartsIndex) + Left.Length;

            var len = Str.IndexOf(Right, i) - i;

            return len < 0 ? null : Str.Substring(i, len);
        }

        public static string ReplaceBetween(this string Str, string Left, string Right, string Replacement)
        {
            var i = Str.IndexOf(Left) + Left.Length;

            var len = Str.IndexOf(Right, i) - i;

            return Str.Remove(i, len).Insert(i, Replacement);
        }

        public static string AppendBetween(this string Str, string Left, string Right, string Append)
        {
            return Str.ReplaceBetween(Left, Right, string.Format("{0}{1}", Str.GetBetween(Left, Right), Append));
        }

        public static string JoinUrlParams(this string Str, string Param)
        {
            return string.Format("{0}{1}{2}", Str, Str.Contains('?') ? "&" : "?", Param);
        }

        public static T Parse<T>(this string Str)
        {
            return (T)Reflection.Parse(Str, typeof(T));
        }

    }
}
