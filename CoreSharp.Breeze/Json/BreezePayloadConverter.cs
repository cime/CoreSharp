using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreSharp.Breeze.Json
{
    public class BreezePayloadConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type != JTokenType.Object)
            {
                return token;
            }

            var data = Activator.CreateInstance(typeof(T));
            var dataProperty = typeof(T).GetProperty("Data");

            if (dataProperty != null)
            {
                dataProperty.SetValue(data, token);
            }
            else
            {
                throw new NotSupportedException($"BreezePayloadConverter requires {typeof(T).Namespace}.{typeof(T).Name} to have Data property of type JToken");
            }

            return data;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
