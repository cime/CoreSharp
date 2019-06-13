using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(string keyName)
        {
            KeyName = keyName;
        }

        public IndexAttribute()
        {
        }

        public string KeyName { get; private set; }

        public bool IsKeySet { get { return !string.IsNullOrEmpty(KeyName); } }
    }
}
