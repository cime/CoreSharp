using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class LazyLoadAttribute : Attribute
    {
        public LazyLoadAttribute(bool enable = true)
        {
            Enabled = enable;
        }

        public bool Enabled { get; private set; }
    }
}
