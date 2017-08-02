
namespace Aggregates.Contracts
{
    public interface IEntity : IEventSourced, IQueryResponse
    {
        IEntity EntityParent { get; set; }
    }
    public interface IEntity<TParent> : IEntity where TParent : IEntity
    {
    }
}
