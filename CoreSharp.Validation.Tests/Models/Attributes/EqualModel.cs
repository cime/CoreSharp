using CoreSharp.Common.Attributes;

namespace CoreSharp.Tests.Validation.Models.Attributes
{
    public class EqualModel
    {
        [Equal("CompareValue")]
        public string Name { get; set; }

        [Equal("CompareValue", IncludePropertyName = true)]
        public string Name2 { get; set; }

        [Equal(ComparsionProperty = "LastNameCompare")]
        public string LastName { get; set; }

        [Equal(ComparsionProperty = "LastNameCompare", IncludePropertyName = true)]
        public string LastName2 { get; set; }

        public string LastNameCompare { get; set; } = "LastNameCompare";
    }
}
