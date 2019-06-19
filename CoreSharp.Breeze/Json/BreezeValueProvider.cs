using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate;

namespace CoreSharp.Breeze.Json
{
    public class BreezeValueProvider : IValueProvider
    {
        private readonly Func<object, IMemberConfiguration, object, object> _serializeFunc;
        private readonly Func<object, IMemberConfiguration, object, object> _deserializeFunc;
        private readonly IValueProvider _valueProvider;
        private readonly IMemberConfiguration _memberConfiguration;

        public BreezeValueProvider(IValueProvider dynamicValueProvider, IMemberConfiguration memberConfiguration)
        {
            _serializeFunc = memberConfiguration.SerializeFunc;
            _deserializeFunc = memberConfiguration.DeserializeFunc;
            _valueProvider = dynamicValueProvider;
            _memberConfiguration = memberConfiguration;
        }

        public void SetValue(object target, object value)
        {
            value = _deserializeFunc != null ? _deserializeFunc(target, _memberConfiguration, value) : value;
            _valueProvider.SetValue(target, value);
        }

        public object GetValue(object target)
        {
            try
            {
                var value = _valueProvider.GetValue(target);
                return (_serializeFunc == null ? value : _serializeFunc(target, _memberConfiguration, value));
            }
            catch (JsonSerializationException ex)
            {
                if(ex.InnerException is LazyInitializationException)
                {
                    //Happens for nonmapped computed properties that touch uninitialized relations when session is closed
                    return null;
                }

                throw;
            }
        }
    }
}
