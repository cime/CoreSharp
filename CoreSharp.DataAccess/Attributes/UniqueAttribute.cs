using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : Attribute
    {
        public UniqueAttribute(string keyName)
        {
            KeyName = keyName;
        }

        public UniqueAttribute(params string[] keyNames)
        {
            KeyName = string.Join(",", keyNames);
        }


        public UniqueAttribute()
        {
        }

        public string KeyName { get; private set; }

        public bool IsKeySet { get { return !string.IsNullOrEmpty(KeyName); } }
    }
}
