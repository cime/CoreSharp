namespace CoreSharp.GraphQL.Models
{
    public class Sort
    {
        public static SortDirection DefaultSortDirection = SortDirection.ASC;

        public string Field { get; set; }
        public SortDirection Direction { get; set; } = DefaultSortDirection;
    }
}
