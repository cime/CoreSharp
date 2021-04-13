using CoreSharp.Cqrs.AspNetCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Http
{
    // ReSharper disable once InconsistentNaming
    public static class IOwinContextExtensions
    {
        public static readonly string ContextKey = "CQRS";

        public static CqrsContext GetCqrsContext(this HttpContext context)
        {
            return context.Items[ContextKey] as CqrsContext;
        }
    }
}
