
namespace Aggregates.Contracts
{
    public interface IEntity : IEventSourced, IQueryResponse
    {
    }
    public interface IEntity<TParent> : IEventSourced where TParent : IEventSource
    {
        new TParent Parent { get; set; }
    }
}
