using CoreSharp.Common.Attributes;

namespace CoreSharp.Validation.Tests.Models.Attributes
{
    public class LessThanOrEqualModel
    {
        [LessThanOrEqual(10)]
        public int Value { get; set; }

        [LessThanOrEqual(10, IncludePropertyName = true)]
        public int Value2 { get; set; }

    }
}
