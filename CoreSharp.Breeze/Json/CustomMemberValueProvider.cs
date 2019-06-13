using System;
using CoreSharp.Breeze.Extensions;
using Newtonsoft.Json.Serialization;

namespace CoreSharp.Breeze.Json
{
    public class CustomMemberValueProvider : IValueProvider
    {
        private readonly Func<object, IMemberConfiguration, object, object> serializeFunc;
        private readonly IMemberConfiguration memberConfiguration;
        private readonly object defaultValue;

        public CustomMemberValueProvider(IMemberConfiguration memberConfiguration)
        {
            this.serializeFunc = memberConfiguration.SerializeFunc;
            this.memberConfiguration = memberConfiguration;
            this.defaultValue = memberConfiguration.MemberType.GetDefaultValue();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public void SetValue(object target, object value)
        {
        }

        public object GetValue(object target)
        {
            return this.serializeFunc == null
                ? this.defaultValue
                : this.serializeFunc(target, this.memberConfiguration, this.defaultValue);
        }
    }
}
