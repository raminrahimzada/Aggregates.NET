using System.Collections.Generic;
using NServiceBus;

namespace Aggregates.Contracts
{
    public interface IEventSource
    {
        Id Id { get; }
        long Version { get; }
        IEventStream Stream { get; }
        IEventSource Parent { get; }
    }

    public interface IEventSourced : IEventSource         
    {

        void Hydrate(IEnumerable<IEvent> events);
        void Conflict(IEvent @event, IDictionary<string, string> metadata = null);
        void Apply(IEvent @event, IDictionary<string, string> metadata = null);
        void Raise(IEvent @event, string id, IDictionary<string, string> metadata = null);
    }

}