using System;

namespace CoreSharp.Common.Attributes
{
    public enum Priority
    {
        Critical = -10000,
        High = -1000,
        Normal = 0,
        Low = 10000
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PriorityAttribute : Attribute
    {
        public int Priority { get; set; }

        public PriorityAttribute(Priority priority)
        {
            Priority = (int)priority;
        }

        public PriorityAttribute(short priority = 0)
        {
            Priority = priority;
        }
    }
}
