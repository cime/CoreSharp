using System.Collections.Generic;

namespace CoreSharp.Cqrs.Mvc
{
    /// <summary>
    /// Model used only for type recognition in controllers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryResult<T>
    {
        public IEnumerable<T> Results { get; set; }

        public int? InlineCount { get; set; }
    }
}
