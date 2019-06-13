using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CoreSharp.Breeze.Extensions
{
    internal static class DictionaryExtensions
    {
        public static T GetValue<T>(this Dictionary<string, object> hashtable, string key)
        {
            return GetValueInternal<T>(hashtable, key);
        }

        public static T GetValue<T>(this IDictionary<string, object> hashtable, string key)
        {
            return GetValueInternal<T>(hashtable, key);
        }

        public static T GetValue<T>(this IDictionary hashtable, string key)
        {
            return GetValueInternal<T>(hashtable, key);
        }

        private static T GetValueInternal<T>(dynamic hashtable, string key)
        {
            if (!hashtable.ContainsKey(key)) return default(T);
            if (hashtable[key] is T) return (T)hashtable[key];
            var jToken = hashtable[key] as JToken;
            if (jToken != null)
            {
                if (typeof(T).IsEnum)
                    hashtable[key] = Enum.Parse(typeof(T), jToken.Value<string>(), true);
                else
                    hashtable[key] = jToken.ToObject<T>();
                return (T)hashtable[key];
            }
            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), hashtable[key].ToString(), true);
            return (T)Convert.ChangeType(hashtable[key], typeof(T));
        }
    }
}
