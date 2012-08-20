using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using DataAvail.Utils.Extensions;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace DataAvail.Utils
{
    public class Reflection
    {
        public static T CreateObject<T>(NameValueCollection PropValCollection) where T : new()
        {
            T obj = new T();

            InitializeObject(obj, PropValCollection);

            return obj;
        }

        public static void InitializeObject(object Object, NameValueCollection PropValCollection)
        {
            for (int i = 0; i < PropValCollection.Count; i++)
            {
                var prop = Object.GetType().GetProperty(PropValCollection.GetKey(i));

                if (prop != null)
                {
                    prop.SetValue(Object, PropValCollection.GetValues(i)[0].ConvertToType(prop.PropertyType), null);
                }
            }
        }

        public static T CastAnonym<T>(object AnonymObject) where T : new()
        {
            T obj = new T();

            foreach (var prop in AnonymObject.GetType().GetProperties())
            {
                obj.GetType().GetProperty(prop.Name).SetValue(obj, prop.GetValue(AnonymObject, null), null);
            }

            return obj;
        }

        public static Dictionary<string, object> GetAnonymProps(object AnonymObject) 
        {
            //http://www.fsmpi.uni-bayreuth.de/~dun3/archives/asp-net-mvc-3-html-actionlink-image-imageactionlink-improved/483.html
            var dictionary = new Dictionary<string, object>();

            if (AnonymObject != null)
            {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(AnonymObject))
                {
                    dictionary.Add(propertyDescriptor.Name, propertyDescriptor.GetValue(AnonymObject));
                }
            }

            return dictionary;

        }

        public static bool IsPrimitive(Type Type, bool NullableAsPrimitive = true)
        {
            return Type.IsPrimitive || Type == typeof(string) || Type == typeof(System.DateTime) || (NullableAsPrimitive && IsNullable(Type));   
        }

        //http://msdn.microsoft.com/en-us/library/ms366789.aspx?ppud=4
        public static bool IsNullable(Type Type)
        {
            return Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        //Return unwrapped type (get rid from Nullable<> shell) if type id nullable, othervice returns just passed type.
        public static Type GetCoreType(Type Type)
        {
            return IsNullable(Type) ? Type.GetGenericArguments()[0] : Type;
        }

        public static Type GetItemType(object Enumerable)
        {
            return Enumerable.GetType().GetGenericArguments()[0];
        }

        public static object GetVal(object Obj, string PropName)
        {
            var obj = Obj;
            
            foreach (var r in PropName.Split('.'))
            {
                if (obj == null)
                    break;

                var t = obj.GetType();

                obj = t.GetProperty(r).GetValue(obj, null);
            }

            return obj;
        }


        public static T GetAnonymVal<T>(object Obj, string PropName, T DefVal)
        {
            var p = Obj.GetType().GetProperty(PropName);

            return p != null ? (T)p.GetValue(Obj, null) : DefVal;
        }

        public static void SetVal(object Obj, string PropName, object Val)
        {
            Obj.GetType().GetProperty(PropName).SetValue(Obj, Val, null);
        }


        public static T GetAttr<T>(object Obj, string PropertyName) where T : Attribute
        {
            return (T)Obj.GetType().GetProperty(PropertyName).GetCustomAttributes(true).FirstOrDefault(p => p is T);
        }

        public static void Copy<T>(T Src, T Dest)
        {
            foreach (string p in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | System.Reflection.BindingFlags.Public).Select(p => p.Name))
            {
                SetVal(Dest, p, GetVal(Src, p));
            }
        }

        public static MethodInfo GetStaticGenericMethod(Type TClass, string MethodName, Type TGenericParam)
        {
            //http://blogs.microsoft.co.il/blogs/bursteg/archive/2006/11/15/InvokeStaticGenericMethod.aspx

            var methodInfo = TClass.GetMethods().Single(p => p.Name == MethodName && p.GetGenericArguments().Count() == 1);

            return methodInfo.MakeGenericMethod(TGenericParam);
        }

        public static object InvokeStaticGenericMethod(Type TClass, string MethodName, Type TGenericParam, params object [] Prms)
        {
            var mi = GetStaticGenericMethod(TClass, MethodName, TGenericParam);

            return mi.Invoke(null, Prms);
        }

        public static object Parse(string StrVal, Type Type)
        {
            var converter = TypeDescriptor.GetConverter(Type);

            return converter.ConvertFrom(StrVal);
        }

        public static object ParsePropertyValue(Type ContainerType, string PropertyName, string PropertyValue)
        {
            return Parse(PropertyValue, ContainerType.GetProperty(PropertyName).PropertyType);
        }

        #region Nestrd Property Info

        public static IEnumerable<NestedPropertyInfo> GetNestedPropertyInfo(Type Type, NestedPropertyInfo Parent)
        {
            foreach (var p in Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField))
            {
                var pi = new NestedPropertyInfo
                {
                    parent = Parent,

                    propertyInfo = p
                };

                pi.nested = GetNestedPropertyInfo((PropertyInfo)p, pi);

                yield return pi;
            }
        }

        public static IEnumerable<NestedPropertyInfo> GetNestedPropertyInfo(object Object)
        {
            return GetNestedPropertyInfo(Object.GetType(), null);
        }

        public static IEnumerable<NestedPropertyInfo> GetNestedPropertyInfo(PropertyInfo ContainerPropertyInfo, NestedPropertyInfo Parent)
        {
            return GetNestedPropertyInfo(ContainerPropertyInfo.PropertyType, Parent);
        }

        public static IEnumerable<PropertyInfo> GetSequencedPropertyAccessRoute(IEnumerable<NestedPropertyInfo> Nest, PropertyInfo PropertyInfo)
        {
            var n = Nest.First(p => p.propertyInfo == PropertyInfo);

            while (n != null)
            {
                yield return n.propertyInfo;

                n = n.parent;
            }
        }

        public static IEnumerable<PropertyInfo> GetSequencedPropertyAccessRoute(object Object, PropertyInfo PropertyInfo)
        {
            var n = GetNestedPropertyInfo(Object).First(p => p.propertyInfo == PropertyInfo);

            while (n != null)
            {
                yield return n.propertyInfo;

                n = n.parent;
            }
        }

        #endregion


        public static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly, Type extendedType)
        {
            //http://stackoverflow.com/questions/299515/c-reflection-to-identify-extension-methods

            var query = from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static
                            | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == extendedType
                        select method;

            return query;
        }

        public static object GetDefault(Type t)
        {
            return typeof(Reflection).GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(null, null);
        }

        public static T GetDefaultGeneric<T>()
        {
            return default(T);
        }
    }

    public class NestedPropertyInfo
    {
        public PropertyInfo propertyInfo;

        public NestedPropertyInfo parent;

        public IEnumerable<NestedPropertyInfo> nested;
    }

}
