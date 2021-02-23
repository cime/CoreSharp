using System.Collections.Generic;

namespace CoreSharp.Common.Models
{
    public class PagedList<T>
    {
        public IEnumerable<T> Results { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }

        public long? InlineCount { get; set; }
    }
}
