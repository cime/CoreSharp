using System;
using CoreSharp.DataAccess.Enums;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class FetchModeAttribute : Attribute
    {
        public FetchModeAttribute(FetchMode mode)
        {
            Mode = mode;
        }

        public FetchMode Mode { get; private set; }
    }
}
