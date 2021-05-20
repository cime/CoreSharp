using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CoreSharp.Mvc.Formatters
{

    public class JsonCamelCaseInputFormatter : JsonInputFormatterBase
    {

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonCamelCaseInputFormatter() : base("CamelCase")
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
            //jsonSerializerSettings.Converters.Add(new TimeSpanConverter());

            // enum as string
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());

            // return
            return jsonSerializerSettings;
        }

        public override async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {

            try
            {
                var request = context.HttpContext.Request;
                string body;
                if (request.Method == HttpMethod.Get.Method || request.HasFormContentType)
                {
                    body = SerializationHelpers.ConvertQueryStringToJson(request.QueryString.Value);
                }
                else
                {
                    using (var stream = new StreamReader(request.Body))
                    {
                        body = await stream.ReadToEndAsync();
                    }
                }

                var obj = JsonConvert.DeserializeObject(body, context.ModelType, _jsonSerializerSettings);
                return await InputFormatterResult.SuccessAsync(obj);
            }
            catch (Exception)
            {
                return await InputFormatterResult.FailureAsync();
            }
        }

    }
   
}
