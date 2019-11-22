using CoreSharp.Common.Attributes;

namespace CoreSharp.Tests.Validation.Models.Attributes
{
    public class GreaterThanModel
    {
        [GreaterThan(10)]
        public int Value { get; set; }

        [GreaterThan(10, IncludePropertyName = true)]
        public int Value2 { get; set; }

    }
}
