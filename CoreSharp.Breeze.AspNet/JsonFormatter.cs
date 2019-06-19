using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;

namespace CoreSharp.Breeze.AspNet
{
    internal class JsonFormatter
    {
        private readonly IBreezeConfig _breezeConfig;

        public JsonFormatter(IBreezeConfig breezeConfig)
        {
            _breezeConfig = breezeConfig;
        }

        public JsonMediaTypeFormatter Create()
        {
            var jsonSerializerSettings = _breezeConfig.GetJsonSerializerSettings();

            var formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings = jsonSerializerSettings;
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            formatter.SupportedEncodings.Add(new UTF8Encoding(false, true));

            return formatter;
        }
    }
}
