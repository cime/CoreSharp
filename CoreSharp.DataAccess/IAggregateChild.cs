namespace CoreSharp.DataAccess
{
    public interface IAggregateChild : IEntity
    {
        IAggregateRoot AggregateRoot { get; }
    }
}
