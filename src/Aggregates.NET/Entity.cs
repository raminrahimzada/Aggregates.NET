using Aggregates.Contracts;

namespace Aggregates
{

    public abstract class Entity<TThis, TState, TParent> : Internal.Entity<TThis, TState>, IEntity<TParent> where TParent : IEntity  where TThis : Entity<TThis, TState, TParent> where TState : class, new()
    {

        public TParent Parent => (TParent)(this as IEntity).EntityParent;
    }
    

    public abstract class EntityWithMemento<TThis, TState, TParent, TMemento> : Entity<TThis, TState, TParent>, ISnapshotting where TMemento : class, IMemento where TParent : IEntity where TThis : EntityWithMemento<TThis, TState, TParent, TMemento> where TState : class, new()
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