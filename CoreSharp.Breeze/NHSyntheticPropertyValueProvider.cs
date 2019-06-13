using Newtonsoft.Json.Serialization;
using NHibernate.Proxy;

namespace CoreSharp.Breeze
{
    public class NHSyntheticPropertyValueProvider : IValueProvider
    {
        private readonly NHSyntheticProperty _syntheticProp;

        public NHSyntheticPropertyValueProvider(NHSyntheticProperty syntheticProp)
        {
            _syntheticProp = syntheticProp;
        }

        public void SetValue(object target, object value)
        {
        }

        /// <summary>
        ///     Get the primary key value
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public object GetValue(object target)
        {
            if (target == null || _syntheticProp == null)
            {
                return null;
            }

            var type = target.GetType();
            var fkPropInfo = type.GetProperty(_syntheticProp.FkPropertyName);

            if (fkPropInfo == null)
            {
                return null;
            }

            var fkValue = fkPropInfo.GetValue(target);

            if (fkValue == null)
            {
                return null;
            }

            var proxy = fkValue as INHibernateProxy;

            if (proxy != null)
            {
                return proxy.HibernateLazyInitializer.Identifier;
            }

            var pkPropInfo = fkValue.GetType().GetProperty(_syntheticProp.PkPropertyName);

            return pkPropInfo == null ? null : pkPropInfo.GetValue(fkValue);
        }
    }
}
