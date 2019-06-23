using System;

namespace CoreSharp.Cqrs.Events
{
    public class UnhandledExceptionEvent : IEvent
    {
        public UnhandledExceptionEvent(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
