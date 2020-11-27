using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace CoreSharp.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>
        {
            foreach (var p in others.SelectMany(src => src))
            {
                me[p.Key] = p.Value;
            }
            return me;
        }

        private static T MergeLeft<T>(this T me, params IDictionary[] others)
            where T : IDictionary<string, object>
        {
            foreach (var dict in others)
            {
                foreach (DictionaryEntry pair in dict)
                {
                    me[pair.Key.ToString()] = pair.Value;
                }
            }
            return me;
        }


        public static IDictionary<string, object> Extend(this IDictionary<string, object> target, params object[] sources)
        {
            //DictionaryEntry
            var idx = 0;
            foreach (var source in sources.Where(o => o != null))
            {
                var dict = source as IDictionary;
                if (dict != null)
                    target.MergeLeft(dict);
                else
                {
                    var type = Nullable.GetUnderlyingType(source.GetType()) ?? source.GetType();

                    //for types that do not have properties add index as key
                    if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                    {
                        target[idx.ToString(CultureInfo.InvariantCulture)] = sources[idx];
                    }
                    else
                    {
                        foreach (var pi in type.GetProperties())
                        {
                            var propValue = pi.GetGetMethod().Invoke(source, null);
                            target[pi.Name] = propValue;
                        }
                    }
                }
                idx++;
            }
            return target;
        }

        /// <summary>
        /// Extension method that turns a dictionary of string and object to an ExpandoObject
        /// </summary>
        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>)expando;

            // go through the items in the dictionary and copy over the key value pairs)
            foreach (var kvp in dictionary)
            {
                // if the value can also be turned into an ExpandoObject, then do it!
                if (kvp.Value is IDictionary<string, object>)
                {
                    var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();
                    expandoDic.Add(kvp.Key, expandoValue);
                }
                else if (kvp.Value is ICollection)
                {
                    // iterate through the collection and convert any strin-object dictionaries
                    // along the way into expando objects
                    var itemList = new List<object>();
                    foreach (var item in (ICollection)kvp.Value)
                    {
                        if (item is IDictionary<string, object>)
                        {
                            var expandoItem = ((IDictionary<string, object>)item).ToExpando();
                            itemList.Add(expandoItem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }

                    expandoDic.Add(kvp.Key, itemList);
                }
                else
                {
                    expandoDic.Add(kvp);
                }
            }

            return expando;
        }
    }
}
