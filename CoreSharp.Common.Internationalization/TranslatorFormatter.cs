using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoreSharp.Common.Extensions;

namespace CoreSharp.Common.Internationalization
{
    public class TranslatorFormatter
    {
        public static string Custom(string format, params object[] args)
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Extend(args);
            format = Regex.Replace(format, @"((?<!{){(?!{)([\w]+)}(?!}))|({({[\w]+})})", match =>
            {
                var escape = match.Groups[3].Value;
                //If double bracers retrun the inside
                if (!string.IsNullOrEmpty(escape))
                {
                    return match.Groups[4].Value;
                }

                var key = match.Groups[2].Value;

                return dictionary.ContainsKey(key) ? dictionary[key].ToString() : match.Groups[1].Value;
            }, RegexOptions.IgnoreCase);
            return format;
        }
    }
}
