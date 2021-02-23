using System.Collections.Generic;

namespace CoreSharp.Common.Models
{
    public class PagedListOrderFilter : PagedListFilter
    {

        public IEnumerable<string> SortBy { get; set; }

        public IEnumerable<bool> SortOrder { get; set; }

    }
}
