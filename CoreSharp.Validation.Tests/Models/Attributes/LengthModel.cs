using CoreSharp.Common.Attributes;

namespace CoreSharp.Validation.Tests.Models.Attributes
{
    public class LengthModel
    {
        [Length(10)]
        public string Name { get; set; }

        [Length(10, IncludePropertyName = true)]
        public string Name2 { get; set; }

        [Length(10, Min =5)]
        public string Nickname { get; set; }

        [Length(10, IncludePropertyName = true, Min = 5)]
        public string Nickname2 { get; set; }
    }
}
