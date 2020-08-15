using Aggregates.Messages;

namespace Aggregates.Contracts
{
    internal interface IEntityFactory<out TEntity> where TEntity : IEntity
    {
        TEntity Create(string bucket, Id id, IParentDescriptor[] parents = null, IEvent[] events = null, object snapshot = null);
    }
}
