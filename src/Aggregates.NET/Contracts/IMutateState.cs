using Aggregates.Messages;

namespace Aggregates.Contracts
{
    internal interface IMutateState
    {
        void Handle(IState state, IEvent @event);
        void Conflict(IState state, IEvent @event);
    }
}
