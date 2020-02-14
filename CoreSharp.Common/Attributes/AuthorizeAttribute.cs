using System;

namespace CoreSharp.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute
    {
        public string[] Permissions { get; set; }

        public AuthorizeAttribute()
        {

        }

        public AuthorizeAttribute(params string[] permission)
        {
            Permissions = permission;
        }
    }
}
