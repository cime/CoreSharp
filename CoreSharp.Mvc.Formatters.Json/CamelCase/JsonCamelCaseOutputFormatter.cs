using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CoreSharp.Mvc.Formatters
{
    public class JsonCamelCaseOutputFormatter : JsonOutputFormatterBase
    {

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonCamelCaseOutputFormatter() : base("CamelCase")
        {
            _jsonSerializerSettings = CreateJsonSerializerSettings();
        }

        private static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Include,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            };

            // Default is DateTimeZoneHandling.RoundtripKind - you can change that here.
            // jsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            // Hack is for the issue described in this post:
            // http://stackoverflow.com/questions/11789114/internet-explorer-json-net-javascript-date-and-milliseconds-issue
            jsonSerializerSettings.Converters.Add(new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-dd\\THH:mm:ss.fffK"
                // DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"
            });
            
            // Needed because JSON.NET does not natively support I8601 Duration formats for TimeSpan
            // jsonSerializerSettings.Converters.Add(new TimeSpanConverter());

            // enum as string
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());

            // return
            return jsonSerializerSettings;
        }

        public override string GetPayload(OutputFormatterWriteContext context)
        {
            var payload = JsonConvert.SerializeObject(context.Object, _jsonSerializerSettings);
            return payload;
        }
    }
}
