using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoreSharp.Common.Extensions
{
    public static class ObjectCollectionExtensions
    {
        public static IEnumerable<T> FindCollectionOfType<T>(this object obj)
            where T : class
        {
            var list = new List<T>();
            FindCollectionOfType(obj, list);
            return list;
        }

        public static IEnumerable FindCollectionOfType(this Type type, object obj)
        {
            var findMethod = typeof(ObjectCollectionExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .SingleOrDefault(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            var result = findMethod.MakeGenericMethod(type).Invoke(null, new object[] { obj });
            return result as IEnumerable;
        }

        public static void FindCollectionOfType<T>(this object obj, IList<T> data, int currentLevel = 0, int maxDepth = 5)
            where T : class
        {
            // not set 
            if (obj == null || maxDepth <= currentLevel)
            {
                return;
            }

            // found list
            var objList = obj as IEnumerable<T>;
            if (objList != null)
            {
                objList.ToList().ForEach(data.Add);
                return;
            }

            // list not of type T
            var objOtherList = obj as IEnumerable;
            if (objOtherList != null)
            {
                var enumerator = objOtherList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var enmObj = enumerator.Current;
                    FindCollectionOfType(enmObj, data, currentLevel + 1, maxDepth);
                }
                return;
            }

            // loop through properties
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead).ToList();
            foreach (var prop in props)
            {
                var propObj = prop.GetValue(obj);
                if (propObj == null)
                {
                    continue;
                }
                FindCollectionOfType(propObj, data, currentLevel + 1, maxDepth);
            }
        }
    }
}
