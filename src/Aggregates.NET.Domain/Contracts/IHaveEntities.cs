

namespace Aggregates.Contracts
{
    public interface IHaveEntities<TParent> where TParent : IEntity
    {
        IRepository<TEntity, TParent> For<TEntity>() where TEntity : IEntity<TParent>;
        IPocoRepository<T, TParent> Poco<T>() where T : class, new();
    }
}