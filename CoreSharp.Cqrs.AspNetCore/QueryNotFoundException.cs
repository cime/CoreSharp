using System;

namespace CoreSharp.Cqrs.AspNetCore
{
    public class QueryNotFoundException : Exception
    {
        public QueryNotFoundException(string message) : base(message)
        {

        }
    }
}
