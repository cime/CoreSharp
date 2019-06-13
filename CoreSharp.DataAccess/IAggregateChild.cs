namespace CoreSharp.DataAccess
{
    public interface IAggregateChild
    {
        IAggregateRoot AggregateRoot { get; }
    }
}
