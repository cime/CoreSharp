namespace CoreSharp.DataAccess
{
    public interface IVersionedEntityWithUser<out TUser, out TId> : IVersionedEntityWithUser<TUser>, IVersionedEntity<TId>
    {
    }

    public interface IVersionedEntityWithUser<out TUser> : IVersionedEntity
    {
        TUser CreatedBy { get; }

        TUser LastModifiedBy { get; }
    }
}
