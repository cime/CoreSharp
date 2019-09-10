namespace CoreSharp.Breeze.Validation
{
    public class BreezeValidationRuleSet
    {
        public const string Default = "default";
        public const string Breeze = "Breeze";

        public static string[] BreezeDefault
        {
            get
            {
                return BreezeValidationRuleSet.Combine("Breeze", "default");
            }
        }

        public static string[] Combine(params string[] rules)
        {
            return rules;
        }
    }
}
