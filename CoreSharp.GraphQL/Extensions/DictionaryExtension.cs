#nullable disable

using System.Collections.Generic;

namespace CoreSharp.GraphQL.Extensions
{
    internal static class DictionaryExtension
    {
        internal static TValue GetValueOrDefault<TKey, TValue>
            (   this IDictionary<TKey, TValue> dictionary,TKey key)
        {
            if (dictionary == null)
            {
                return default;
            }

            return dictionary.ContainsKey(key) ? dictionary[key] : default;
        }
    }
}
