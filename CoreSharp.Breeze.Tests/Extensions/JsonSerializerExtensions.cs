using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CoreSharp.Breeze.Tests.Extensions
{
    public static class JsonSerializerExtensions
    {
        public static string Serialize(this JsonSerializer jsonSerializer, object value, Formatting? formatting = Formatting.Indented)
        {
            var sb = new StringBuilder(256);
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = formatting ?? jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value);
            }

            return sw.ToString();
        }

        public static T Deserialize<T>(this JsonSerializer jsonSerializer, string json)
        {
            using var jsonReader = new JsonTextReader(new StringReader(json));
            return jsonSerializer.Deserialize<T>(jsonReader);
        }
    }
}
