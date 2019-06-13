using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValueAttribute : Attribute
    {
        public string Value { get; private set; }

        public DefaultValueAttribute(string value)
        {
            Value = value;
        }

        public DefaultValueAttribute(int value)
        {
            Value = value.ToString();
        }

        public DefaultValueAttribute(bool value)
        {
            Value = value ? "1" : "0";
        }
    }
}
