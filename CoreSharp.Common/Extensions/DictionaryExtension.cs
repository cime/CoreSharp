#nullable disable

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public static class DictionaryExtension
    {
        public static TValue GetValueOrDefault<TKey, TValue>
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
