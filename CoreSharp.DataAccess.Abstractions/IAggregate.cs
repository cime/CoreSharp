namespace CoreSharp.DataAccess
{
    public interface IAggregate : IEntity
    {
        IEntity GetAggregateRoot();
    }
}
