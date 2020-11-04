using Breeze.NHibernate.Serialization;
using Newtonsoft.Json;

namespace CoreSharp.Breeze.Serialization
{
    public class BreezeJsonSerializerSettingsProvider : DefaultJsonSerializerSettingsProvider
    {
        public BreezeJsonSerializerSettingsProvider(BreezeContractResolver breezeContractResolver) : base(breezeContractResolver)
        {
        }

        protected override JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var settings = base.CreateJsonSerializerSettings();
            settings.Converters.Add(new BreezeSaveBundleConverter());

            return settings;
        }
    }
}
