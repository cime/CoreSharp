using System;
using CoreSharp.Cqrs.Mvc.Parameters;
using CoreSharp.Cqrs.Query;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.Cqrs.Mvc.Queries
{

    [ApiController]
    [Route("/api/query/[controller]", Order = int.MaxValue)]
    public abstract class QueryControllerBase<TQuery, THandler, TResult> : ControllerBase
        where TQuery : IQuery<TResult>
        where THandler : IQueryHandler<TQuery, TResult>
    {

        private readonly IQueryProcessor _queryProcessor;

        public QueryControllerBase(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        [HttpPost]
        [Route("")]
        public ActionResult<TResult> HandleRequestBody([FromBody] TQuery req)
        {
            return HandleRequestProcedure(req);
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult<TResult> HandleRequestQuery([FromQuery] TQuery req)
        {
            return HandleRequestProcedure(req);
        }

        private ActionResult<TResult> HandleRequestProcedure(TQuery req)
        {
            // bind query parameters
            HttpContext.BindQueryParams(req);

            try
            {
                var rsp = _queryProcessor.Handle(req);
                if (rsp == null)
                {
                    return NotFound();
                }
                return Ok(rsp);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.ToValidationErrorResponse());
            }

        }

    }
}
