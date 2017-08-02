using Aggregates.Internal;
using NServiceBus.ObjectBuilder;

namespace Aggregates.Contracts
{
    public interface IRepositoryFactory
    {
        IRepository<T> ForAggregate<T>(IBuilder builder) where T : IEntity;
        IRepository<TEntity, TParent> ForEntity<TEntity, TParent>(TParent parent, IBuilder builder) where TEntity : IEntity<TParent> where TParent : IEntity;
        IPocoRepository<T> ForPoco<T>(IBuilder builder) where T : class, new();
        IPocoRepository<T, TParent> ForPoco<T, TParent>(TParent parent, IBuilder builder) where T : class, new() where TParent : IEntity;
    }
}