using CoreSharp.Common.Attributes;
using CoreSharp.Validation.Attributes;

namespace CoreSharp.Tests.Validation.Models.Attributes
{
    [IgnoreValidationAttributes(Properties = new []{"Name"})]
    public class IgnoreValidationAttributesModel
    {
        [NotNull]
        public string Name { get; set; }
    }
}
