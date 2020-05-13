using CoreSharp.Common.Attributes;

namespace CoreSharp.Validation.Tests.Models.Attributes
{
    public class GreaterThanOrEqualModel
    {
        [GreaterThanOrEqual(10)]
        public int Value { get; set; }

        [GreaterThanOrEqual(10, IncludePropertyName = true)]
        public int Value2 { get; set; }

    }
}
