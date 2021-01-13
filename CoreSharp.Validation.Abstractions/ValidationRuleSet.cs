namespace CoreSharp.Validation
{
    public class ValidationRuleSet
    {
        private const string InsertRule = "Insert";
        private const string UpdateRule = "Update";

        public const string Default = "default";

        public const string Attribute = "Attribute";

        public const string Delete = "Delete";

        public static string[] Insert => Combine(Attribute, InsertRule, Default);

        public static string[] InsertUpdate => Combine(Attribute, InsertRule, UpdateRule, Default);

        public static string[] Update => Combine(Attribute, UpdateRule, Default);

        public static string[] Combine(params string[] rules)
        {
            return rules;
        }
    }
}
