using System;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.Cqrs.Mvc.Commands
{

    [ApiController]
    [Route("/api/command/[controller]", Order = int.MaxValue)]
    public abstract class CommandControllerAsyncReturnBase<TCmd, THandler, TResult> : ControllerBase
        where TCmd : IAsyncCommand<TResult>
        where THandler : IAsyncCommandHandler<TCmd, TResult>
    {

        private readonly ICommandDispatcher _commandDispatcher;

        public CommandControllerAsyncReturnBase(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<TResult>> HandleRequest([FromBody] TCmd req, CancellationToken cancellationToken)
        {
            try
            {
                var rsp = await _commandDispatcher.DispatchAsync(req, cancellationToken);
                if (rsp == null)
                {
                    return NoContent();
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
