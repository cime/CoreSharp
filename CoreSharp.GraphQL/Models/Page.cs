namespace CoreSharp.GraphQL.Models
{
    public class Page
    {
        public static int DefaultSize = 20;

        public int Number { get; set; }
        public int Size { get; set; } = DefaultSize;
    }
}
