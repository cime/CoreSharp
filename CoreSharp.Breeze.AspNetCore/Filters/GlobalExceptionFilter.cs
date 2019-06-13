using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreSharp.Breeze.AspNetCore.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter {

        public GlobalExceptionFilter() {

        }

        public void OnException(ExceptionContext context) {
            var ex = context.Exception;
            var msg = ex.InnerException == null ? ex.Message : ex.Message + "--" + ex.InnerException.Message;

            var statusCode = 500;
            var response = new ErrorDto() {
                Message = msg,
                StackTrace = context.Exception.StackTrace

            };

            if (ex is EntityErrorsException eeEx)
            {
                response.Code = (int)eeEx.StatusCode;
                response.EntityErrors = eeEx.EntityErrors;
                statusCode = response.Code;
            }

            context.Result = new ObjectResult(response) {
                StatusCode = statusCode,
                DeclaredType = typeof(ErrorDto)
            };

        }
    }
}
