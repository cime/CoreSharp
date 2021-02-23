namespace CoreSharp.Common.Models
{
    public class PagedListFilter
    {
        public int Skip { get; set; }

        public int Take { get; set; }

        public bool InlineCount { get; set; }
    }
}
