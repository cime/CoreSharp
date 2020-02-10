namespace CoreSharp.Validation
{
    public class ValidationRuleSet
    {
        public const string Default = "default";
        public const string Delete = "Delete";
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Attribute = "Attribute";

        public static string[] AttributeInsert => Combine(Attribute, Insert);

        public static string[] AttributeInsertDefault => Combine(Attribute, Insert, Default);

        public static string[] AttributeInsertUpdate => Combine(Attribute, Insert, Update);

        public static string[] AttributeInsertUpdateDefault => Combine(Attribute, Insert, Update, Default);

        public static string[] AttributeUpdate => Combine(Attribute, Update);

        public static string[] AttributeUpdateDefault => Combine(Attribute, Update, Default);

        public static string[] Combine(params string[] rules)
        {
            return rules;
        }
    }
}
