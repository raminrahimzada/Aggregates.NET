using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Aggregates.Contracts;
using Aggregates.Extensions;
using Aggregates.Messages;

namespace Aggregates.Internal
{
    static class EntityFactory
    {
        private static readonly ConcurrentDictionary<Type, object> Factories = new ConcurrentDictionary<Type, object>();

        public static IEntityFactory<TEntity> For<TEntity>() where TEntity : IEntity
        {
            var factory = Factories.GetOrAdd(typeof(TEntity), t => CreateFactory<TEntity>());

            return factory as IEntityFactory<TEntity>;
        }

        private static IEntityFactory<TEntity> CreateFactory<TEntity>()
            where TEntity : IEntity
        {
            var stateType = typeof(TEntity).BaseType.GetGenericArguments()[1];
            var factoryType = typeof(EntityFactory<,>).MakeGenericType(typeof(TEntity), stateType);

            return Activator.CreateInstance(factoryType) as IEntityFactory<TEntity>;
        }
    }

    class EntityFactory<TEntity, TState> : IEntityFactory<TEntity> where TEntity : Entity<TEntity, TState> where TState: IState, new()
    {
        readonly Func<TEntity> _factory;

        public EntityFactory()
        {
            _factory = ReflectionExtensions.BuildCreateEntityFunc<TEntity>();
        }

        public TEntity CreateNew(Id id)
        {
            var entity = _factory();
            var state = new TState();

            entity.Id = id;
            entity.State = state;

            return entity;
        }

        public TEntity CreateFromEvents(Id id, IFullEvent[] events, IState snapshot = null)
        {
            var entity = _factory();
            var state = new TState();

            if (snapshot != null)
                state.RestoreSnapshot(snapshot);

            for (var i = 0; i < events.Length; i++)
                state.Apply(events[i].Event);

            entity.State = state;

            return entity;
        }
    }
}
