using CoreSharp.Mvc.Formatters;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MvcExtensionsJsonCamelCase
    {
        public static IMvcBuilder AddJsonCamelCaseFormatters(this IMvcBuilder mvc)
        {
            mvc.AddMvcOptions(opt => opt.InputFormatters.Insert(0, new JsonCamelCaseInputFormatter()));
            mvc.AddMvcOptions(opt => opt.OutputFormatters.Insert(0, new JsonCamelCaseOutputFormatter()));
            return mvc;
        }
    }
}
