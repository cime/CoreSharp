namespace CoreSharp.GraphQL.Models
{
    public class Order
    {
        public static Direction DefaultDirection = Direction.ASC;

        public string Field { get; set; }
        public Direction Direction { get; set; } = DefaultDirection;
    }
}
