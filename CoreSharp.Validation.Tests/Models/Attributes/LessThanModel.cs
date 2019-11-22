using CoreSharp.Common.Attributes;

namespace CoreSharp.Tests.Validation.Models.Attributes
{
    public class LessThanModel
    {
        [LessThan(10)]
        public int Value { get; set; }

        [LessThan(10, IncludePropertyName = true)]
        public int Value2 { get; set; }

    }
}
