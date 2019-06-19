using System;

namespace CoreSharp.Cqrs.AspNetCore
{
    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException(string message) : base(message)
        {

        }
    }
}
