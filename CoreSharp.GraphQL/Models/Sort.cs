namespace CoreSharp.GraphQL.Models
{
    public class Sort
    {
        public static SortDirection DefaultSortDirection = SortDirection.ASC;

        public string Field { get; set; }
        public SortDirection SortDirection { get; set; } = DefaultSortDirection;
    }
}
