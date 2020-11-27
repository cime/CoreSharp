using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HiLoAttribute : Attribute
    {
        public int Size { get; set; } = 100;

        public HiLoAttribute()
        {

        }

        public HiLoAttribute(int size)
        {
            Size = size;
        }
    }
}
