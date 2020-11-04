using System;
using Breeze.NHibernate;
using FluentNHibernate.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreSharp.Breeze.Serialization
{
    public class BreezeSaveBundleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.HasInterface(typeof(IBreezeSaveBundle));
        }

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type != JTokenType.Object)
            {
                return token;
            }

            var tag = token.SelectToken("saveOptions.tag", false);
            IBreezeSaveBundle result;

            if (tag != null)
            {
                result = (IBreezeSaveBundle) tag.ToObject(objectType);
            }
            else
            {
                result = (IBreezeSaveBundle) Activator.CreateInstance(objectType);
            }

            result.SaveBundle = token.ToObject<SaveBundle>();

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
