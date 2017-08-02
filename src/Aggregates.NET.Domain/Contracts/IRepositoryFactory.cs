using Aggregates.Internal;
using NServiceBus.ObjectBuilder;

namespace Aggregates.Contracts
{
    public interface IRepositoryFactory
    {
        IRepository<T> ForAggregate<T>(IBuilder builder) where T : IEventSource;
        IRepository<TEntity, TParent> ForEntity<TEntity, TParent>(TParent parent, IBuilder builder) where TEntity : IEventSource where TParent : IEventSource;
        IPocoRepository<T> ForPoco<T>(IBuilder builder) where T : class, new();
        IPocoRepository<T, TParent> ForPoco<T, TParent>(TParent parent, IBuilder builder) where T : class, new() where TParent : IEventSource;
    }
}