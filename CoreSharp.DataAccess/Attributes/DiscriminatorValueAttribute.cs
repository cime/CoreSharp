using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DiscriminatorValueAttribute : Attribute
    {
        public string Value { get; }

        public DiscriminatorValueAttribute(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
        }
    }
}
