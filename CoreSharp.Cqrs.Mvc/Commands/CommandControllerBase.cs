using System;
using CoreSharp.Cqrs.Command;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.Cqrs.Mvc.Commands
{

    [ApiController]
    [Route("/api/command/[controller]", Order = int.MaxValue)]
    public abstract class CommandControllerBase<TCmd, THandler> : ControllerBase
        where TCmd : ICommand
        where THandler : ICommandHandler<TCmd>
    {

        private readonly ICommandDispatcher _commandDispatcher;

        public CommandControllerBase(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]
        [Route("")]
        public ActionResult HandleRequest([FromBody] TCmd req)
        {
            try
            {
                _commandDispatcher.Dispatch(req);
                return NoContent();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.ToValidationErrorResponse());
            }
        }
    }
}
