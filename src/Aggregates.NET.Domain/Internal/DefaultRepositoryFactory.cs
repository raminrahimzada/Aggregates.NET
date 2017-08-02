using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Aggregates.Contracts;
using NServiceBus.ObjectBuilder;

namespace Aggregates.Internal
{
    class DefaultRepositoryFactory : IRepositoryFactory
    {
        private static readonly ConcurrentDictionary<Type, Type> RepoCache = new ConcurrentDictionary<Type, Type>();

        public IRepository<TEntity> ForAggregate<TEntity>(IBuilder builder) where TEntity : IEventSource
        {
            var repoType = RepoCache.GetOrAdd(typeof(TEntity), (key) => typeof(Repository<>).MakeGenericType(typeof(TEntity)));

            return (IRepository<TEntity>)Activator.CreateInstance(repoType, builder);
        }
        public IRepository<TEntity, TParent> ForEntity<TEntity, TParent>(TParent parent, IBuilder builder) where TEntity : IEventSource where TParent : IEventSource
        {
            var repoType = RepoCache.GetOrAdd(typeof(TEntity), (key) => typeof(Repository<,>).MakeGenericType(typeof(TParent), typeof(TEntity)));

            return (IRepository<TEntity, TParent>)Activator.CreateInstance(repoType, parent, builder);
            
        }
        public IPocoRepository<T> ForPoco<T>(IBuilder builder) where T : class, new()
        {
            var repoType = RepoCache.GetOrAdd(typeof(T), (key) => typeof(PocoRepository<>).MakeGenericType(typeof(T)));

            return (IPocoRepository<T>)Activator.CreateInstance(repoType, builder);
        }
        public IPocoRepository<T, TParent> ForPoco<T, TParent>(TParent parent, IBuilder builder) where T : class, new() where TParent : IEventSource
        {
            var repoType = RepoCache.GetOrAdd(typeof(T), (key) => typeof(PocoRepository<,>).MakeGenericType(typeof(TParent), typeof(T)));

            return (IPocoRepository<T, TParent>)Activator.CreateInstance(repoType, parent, builder);
        }
    }
}