using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.Specialized;

namespace DataAvail.Utils.Extensions
{
    public static class XmlLinq
    {
        //http://www.hanselman.com/blog/GetNamespacesFromAnXMLDocumentWithXPathDocumentAndLINQToXML.aspx
        public static IDictionary<string, XNamespace> GetNamespacesInScope(this XElement XElement)
        {
            return XElement.Attributes().
                    Where(a => a.IsNamespaceDeclaration).
                    GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                            a => XNamespace.Get(a.Value)).
                    ToDictionary(g => g.Key,
                                 g => g.First());
        }

        public static string GetAttribute(this XElement Element, string AttributeName)
        {
            return GetAttribute(Element, AttributeName, false);
        }

        public static string GetAttributeSafe(this XElement Element, string AttributeName)
        {
            return GetAttribute(Element, AttributeName, true);
        }

        public static string GetAttribute(this XElement Element, string AttributeName, bool CheckElementNull)
        {
            if (CheckElementNull && Element == null) return null;

            XAttribute attr = Element.Attributes().FirstOrDefault(p => p.Name == AttributeName);

            return attr != null ? attr.Value : null;
        }

        public static T Format<T>(this XElement XElement)
            where T : new()
        {
            var nvc = new NameValueCollection();

            foreach (XElement x in XElement.Elements())
            {
                nvc.Add(x.Name.LocalName, x.Value);
            }

            return Reflection.CreateObject<T>(nvc);
        }
    }
}
