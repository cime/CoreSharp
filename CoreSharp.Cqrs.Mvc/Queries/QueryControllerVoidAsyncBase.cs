using System;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Query;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.Cqrs.Mvc.Queries
{

    [ApiController]
    [Route("/api/query/[controller]", Order = int.MaxValue)]
    public abstract class QueryControllerVoidAsyncBase<TQuery, THandler, TResult> : ControllerBase
        where TQuery : IAsyncQuery<TResult>
        where THandler : IAsyncQueryHandler<TQuery, TResult>
    {

        private readonly IQueryProcessor _queryProcessor;

        public QueryControllerVoidAsyncBase(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<TResult>> HandleRequestBody(CancellationToken cancellationToken)
        {
            return await HandleRequestProcedure(cancellationToken);
        }

        [HttpGet]
        [Route("")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<TResult>> HandleRequestQuery(CancellationToken cancellationToken)
        {
            return await HandleRequestProcedure(cancellationToken);
        }

        private async Task<ActionResult<TResult>> HandleRequestProcedure(CancellationToken cancellationToken)
        {

            try
            {
                var req = Activator.CreateInstance<TQuery>();
                var rsp = await _queryProcessor.HandleAsync(req, cancellationToken);
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
