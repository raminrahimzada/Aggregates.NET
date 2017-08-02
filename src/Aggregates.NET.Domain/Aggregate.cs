using Aggregates.Contracts;
using NServiceBus.Logging;

namespace Aggregates
{
    public abstract class Aggregate<TThis, TState> : Internal.Entity<TThis, TState> where TThis : Aggregate<TThis, TState> where TState : class, IState, new()
    {
    }

    public abstract class AggregateWithMemento<TThis, TState, TMemento> : Aggregate<TThis, TState>, ISnapshotting where TMemento : class, IMemento where TThis : AggregateWithMemento<TThis, TState, TMemento> where TState : class, IState, new()
    {
        ISnapshot ISnapshotting.Snapshot => Stream.Snapshot;

        void ISnapshotting.RestoreSnapshot(IMemento snapshot)
        {
            RestoreSnapshot(snapshot as TMemento);
        }

        IMemento ISnapshotting.TakeSnapshot()
        {
            return TakeSnapshot();
        }

        bool ISnapshotting.ShouldTakeSnapshot()
        {
            return ShouldTakeSnapshot();
        }

        public TMemento Snapshot => (this as ISnapshotting).Snapshot?.Payload as TMemento;

        protected abstract void RestoreSnapshot(TMemento memento);

        protected abstract TMemento TakeSnapshot();

        protected abstract bool ShouldTakeSnapshot();
        
    }
}