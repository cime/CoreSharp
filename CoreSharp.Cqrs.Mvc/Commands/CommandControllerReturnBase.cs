using System;
using CoreSharp.Cqrs.Command;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.Cqrs.Mvc.Commands
{

    [ApiController]
    [Route("/api/command/[controller]", Order = int.MaxValue)]
    public abstract class CommandControllerReturnBase<TCmd, THandler, TResult> : ControllerBase
        where TCmd : ICommand<TResult>
        where THandler : ICommandHandler<TCmd, TResult>
    {

        private readonly ICommandDispatcher _commandDispatcher;

        public CommandControllerReturnBase(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]
        [Route("")]
        public ActionResult<TResult> HandleRequest([FromBody] TCmd req)
        {
            try
            {
                var rsp = _commandDispatcher.Dispatch(req);
                if(rsp == null)
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
