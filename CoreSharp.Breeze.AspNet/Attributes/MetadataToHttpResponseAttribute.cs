using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CoreSharp.Breeze.AspNet.Attributes
{
/// <summary>
    /// Converts output of a method named "Metadata" that returns a string
    /// into an HttpResponse with string content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class MetadataToHttpResponseAttribute : ActionFilterAttribute
    {

        /// <summary>
        /// Called when the action is executed.
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context.</param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {

            base.OnActionExecuted(actionExecutedContext);
            var response = actionExecutedContext.Response;
            if (response == null || !response.IsSuccessStatusCode)
            {
                return;
            }
            var actionContext = actionExecutedContext.ActionContext;
            var actionDescriptor = actionContext.ActionDescriptor;

            if (actionDescriptor.ReturnType == typeof(string) &&
                (actionDescriptor.ActionName == "Metadata" ||
                 // or attribute applied directly to the method
                 0 < actionDescriptor.GetCustomAttributes<MetadataToHttpResponseAttribute>().Count))
            {
                string contentValue;
                if (response.TryGetContentValue(out contentValue))
                {
                    var newResponse = new HttpResponseMessage { Content = new StringContent(contentValue) };
                    newResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    actionContext.Response = newResponse;
                }
            }
        }
    }

    internal class MetadataFilterProvider : IFilterProvider
    {

        public MetadataFilterProvider(MetadataToHttpResponseAttribute filter)
        {
            _filter = filter;
        }

        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            return new[] { new FilterInfo(_filter, FilterScope.Controller) };
        }

        private readonly MetadataToHttpResponseAttribute _filter;
    }
}
