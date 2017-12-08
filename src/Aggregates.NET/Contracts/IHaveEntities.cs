using Aggregates.Internal;

namespace Aggregates.Contracts
{
    interface IHaveEntities<TParent> where TParent : class, IEntity
    {
        IRepository<T, TParent> For<T>() where T : class, IChildEntity<TParent>;
        IPocoRepository<T, TParent> Poco<T>() where T : class, new();
    }
}