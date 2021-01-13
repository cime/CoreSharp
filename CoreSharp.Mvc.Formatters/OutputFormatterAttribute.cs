using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreSharp.Mvc.Formatters
{

    /// <summary>
    /// Adds output formatter name to http context
    /// </summary>
    public class OutputFormatterAttribute : ActionFilterAttribute
    {

        public OutputFormatterAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.SetFormatterName(Name);
            await next();
        }

    }
}
