using CoreSharp.Common.Attributes;

namespace CoreSharp.Tests.Validation.Models.Attributes
{
    public class NotEqualModel
    {
        [NotEqual("CompareValue")]
        public string Name { get; set; }

        [NotEqual("CompareValue", IncludePropertyName = true)]
        public string Name2 { get; set; }

        [NotEqual(ComparsionProperty = "LastNameCompare")]
        public string LastName { get; set; }

        [NotEqual(ComparsionProperty = "LastNameCompare", IncludePropertyName = true)]
        public string LastName2 { get; set; }

        public string LastNameCompare { get; set; } = "LastNameCompare";
    }
}
