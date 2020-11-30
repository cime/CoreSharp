using System.Collections.Generic;

namespace CoreSharp.Cqrs.Mvc.Parameters
{
    internal class Query
    {
        public int? Skip { get; set; }

        public int? Take { get; set; }

        public bool? InlineCount { get; set; }

        public IEnumerable<string> SortBy { get; set; }

        public IEnumerable<bool> SortOrder { get; set; }

    }

}
