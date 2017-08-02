

namespace Aggregates.Contracts
{
    public interface IHaveEntities<TParent> where TParent : IEventSource
    {
        IRepository<TEntity, TParent> For<TEntity>() where TEntity : IEventSource;
        IPocoRepository<T, TParent> Poco<T>() where T : class, new();
    }
}