using CoreSharp.Common.Attributes;

namespace CoreSharp.Tests.Validation.Models.Attributes
{
    public class CreditCardModel
    {
        [CreditCard]
        public string CardNumber { get; set; }

        [CreditCard(IncludePropertyName = true)]
        public string CardNumber2 { get; set; }
    }
}
