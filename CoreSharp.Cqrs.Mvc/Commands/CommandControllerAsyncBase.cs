using System;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Command;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.Cqrs.Mvc.Commands
{

    [ApiController]
    [Route("/api/command/[controller]", Order = int.MaxValue)]
    public abstract class CommandControllerAsyncBase<TCmd, THandler> : ControllerBase
        where TCmd : IAsyncCommand
        where THandler : IAsyncCommandHandler<TCmd>
    {

        private readonly ICommandDispatcher _commandDispatcher;

        public CommandControllerAsyncBase(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult> HandleRequest([FromBody] TCmd req, CancellationToken cancellationToken)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(req, cancellationToken);
                return NoContent();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.ToValidationErrorResponse());
            }
        }
    }
}
