using System;
using CoreSharp.Cqrs.Query;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.Cqrs.Mvc.Queries
{

    [ApiController]
    [Route("/api/query/[controller]", Order = int.MaxValue)]
    public abstract class QueryControllerVoidBase<TQuery, THandler, TResult> : ControllerBase
        where TQuery : IQuery<TResult>
        where THandler : IQueryHandler<TQuery, TResult>
    {

        private readonly IQueryProcessor _queryProcessor;

        public QueryControllerVoidBase(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        [HttpPost]
        [Route("")]
        public ActionResult<TResult> HandleRequestBody()
        {
            return HandleRequestProcedure();
        }

        [HttpGet]
        [Route("")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult<TResult> HandleRequestQuery()
        {
            return HandleRequestProcedure();
        }

        private ActionResult<TResult> HandleRequestProcedure()
        {
            try
            {
                var req = Activator.CreateInstance<TQuery>();
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
