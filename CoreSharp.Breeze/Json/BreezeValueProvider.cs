using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate;

namespace CoreSharp.Breeze.Json
{
    public class BreezeValueProvider : IValueProvider
    {
        private readonly Func<object, IMemberConfiguration, object, object> serializeFunc;
        private readonly Func<object, IMemberConfiguration, object, object> deserializeFunc;
        private readonly IValueProvider valueProvider;
        private readonly IMemberConfiguration memberConfiguration;

        public BreezeValueProvider(IValueProvider dynamicValueProvider, IMemberConfiguration memberConfiguration)
        {
            this.serializeFunc = memberConfiguration.SerializeFunc;
            this.deserializeFunc = memberConfiguration.DeserializeFunc;
            this.valueProvider = dynamicValueProvider;
            this.memberConfiguration = memberConfiguration;
        }

        public void SetValue(object target, object value)
        {
            value = this.deserializeFunc != null ? this.deserializeFunc(target, this.memberConfiguration, value) : value;
            this.valueProvider.SetValue(target, value);
        }

        public object GetValue(object target)
        {
            try
            {
                var value = this.valueProvider.GetValue(target);
                return (this.serializeFunc == null ? value : this.serializeFunc(target, this.memberConfiguration, value));
            }
            catch (JsonSerializationException ex)
            {
                if(ex.InnerException is LazyInitializationException)
                    //Happens for nonmapped computed properties that touch uninitialized relations when session is closed 
                    return null;
                throw;
            }
        }
    }
}
