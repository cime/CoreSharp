namespace CoreSharp.Validation
{
    public class ValidationRuleSet
    {
        public const string Default = "default";
        public const string Delete = "Delete";
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Attribute = "Attribute";

        public static string[] AttributeInsertUpdate => Combine("Attribute", "Insert", "Update");

        public static string[] AttributeInsert => Combine("Attribute", "Insert");

        public static string[] AttributeUpdate => Combine("Update", "Attribute");

        public static string[] AttributeInsertUpdateDefault => Combine("default", "Attribute", "Insert", "Update");

        public static string[] InsertUpdate => Combine("Update", "Insert");

        public static string[] Combine(params string[] rules)
        {
            return rules;
        }
    }
}
