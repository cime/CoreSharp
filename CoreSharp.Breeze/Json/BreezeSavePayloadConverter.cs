using System;
using FluentNHibernate.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreSharp.Breeze.Json
{
    public class BreezeSavePayloadConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.HasInterface(typeof(IBreezeSavePayload));
        }

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type != JTokenType.Object)
            {
                return token;
            }

            var result = (IBreezeSavePayload)Activator.CreateInstance(objectType);
            result.Payload = (JObject) token;

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
