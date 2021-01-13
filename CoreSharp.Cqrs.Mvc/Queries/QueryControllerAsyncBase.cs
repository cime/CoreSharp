using System;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Mvc.Parameters;
using CoreSharp.Cqrs.Query;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.Cqrs.Mvc.Queries
{

    [ApiController]
    [Route("/api/query/[controller]",Order = int.MaxValue)]
    public abstract class QueryControllerAsyncBase<TQuery, THandler, TResult> : ControllerBase
        where TQuery : IAsyncQuery<TResult>
        where THandler : IAsyncQueryHandler<TQuery, TResult>
    {

        private readonly IQueryProcessor _queryProcessor;

        public QueryControllerAsyncBase(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<TResult>> HandleRequestBody([FromBody] TQuery req, CancellationToken cancellationToken)
        {
            return await HandleRequestProcedure(req, cancellationToken);
        }

        [HttpGet]
        [Route("")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<TResult>> HandleRequestQuery([FromQuery] TQuery req, CancellationToken cancellationToken)
        {
            return await HandleRequestProcedure(req, cancellationToken);
        }

        private async Task<ActionResult<TResult>> HandleRequestProcedure(TQuery req, CancellationToken cancellationToken)
        {

            // bind query parameters
            HttpContext.BindQueryParams(req);

            try
            {

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
