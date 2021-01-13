namespace CoreSharp.Mvc.Formatters
{
    public static class HttpContextExtensions
    {
        private static string _formatterNameKey = "FormatterName";

        public static string DefaultFormatter = "Breeze";

        public static void SetFormatterName(this Microsoft.AspNetCore.Http.HttpContext context, string formatterName)
        {

            if(string.IsNullOrWhiteSpace(formatterName))
            {
                formatterName = DefaultFormatter;
            }

            if(context.Items.ContainsKey(_formatterNameKey))
            {
                context.Items[_formatterNameKey] = formatterName;
            } else
            {
                context.Items.Add(_formatterNameKey, formatterName);
            }
        }

        public static string GetFormatterName(this Microsoft.AspNetCore.Http.HttpContext context)
        {
            if (context.Items.ContainsKey(_formatterNameKey))
            {
                return context.Items[_formatterNameKey] as string;
            }
            return null;
        }
    }
}
