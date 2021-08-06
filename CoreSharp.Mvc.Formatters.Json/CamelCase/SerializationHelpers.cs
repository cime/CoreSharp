﻿using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoreSharp.Mvc.Formatters
{
    internal static class SerializationHelpers
    {
        public static string ConvertQueryStringToJson(string query)
        {
            var collection = HttpUtility.ParseQueryString(query);
            var dictionary = collection.AllKeys.Where(x => !string.IsNullOrWhiteSpace(x)).ToDictionary(key => key, key => collection[key]);

            return ConvertDictionaryToJson(dictionary);
        }

        private static string ConvertDictionaryToJson(Dictionary<string, string> dictionary)
        {
            var propertyNames =
                from key in dictionary.Keys
                let index = key.IndexOf('.')
                select index < 0 ? key : key.Substring(0, index);

            var data =
                from propertyName in propertyNames.Distinct()
                let json = dictionary.ContainsKey(propertyName)
                    ? HttpUtility.JavaScriptStringEncode(dictionary[propertyName], true)
                    : ConvertDictionaryToJson(FilterByPropertyName(dictionary, propertyName))
                select HttpUtility.JavaScriptStringEncode(propertyName, true) + ": " + json;

            return "{ " + string.Join(", ", data) + " }";
        }

        private static Dictionary<string, string> FilterByPropertyName(Dictionary<string, string> dictionary, string propertyName)
        {
            var prefix = propertyName + ".";

            return dictionary.Keys
                .Where(key => key.StartsWith(prefix))
                .ToDictionary(key => key.Substring(prefix.Length), key => dictionary[key]);
        }

    }
}
