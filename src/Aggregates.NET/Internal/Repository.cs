﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Aggregates.Exceptions;
using Aggregates.Extensions;
using Aggregates.Logging;

namespace Aggregates.Internal
{
    public class Repository<TEntity, TState, TParent, TStateParent> : Repository<TEntity, TState>, IRepository<TEntity, TParent> where TParent : Entity<TParent, TStateParent> where TEntity : Entity<TEntity, TState, TParent> where TState : State<TState, TStateParent>, new() where TStateParent : State<TStateParent>, new()
    {
        private static readonly ILog Logger = LogProvider.GetLogger("Repository");

        private readonly TParent _parent;

        public Repository(TParent parent, IStoreEntities store)
            : base(store)
        {
            _parent = parent;
        }

        public override async Task<TEntity> TryGet(Id id)
        {
            if (id == null) return default(TEntity);
            try
            {
                return await Get(id).ConfigureAwait(false);
            }
            catch (NotFoundException) { }
            return default(TEntity);

        }
        public override async Task<TEntity> Get(Id id)
        {
            var cacheId = $"{_parent.Bucket}.{_parent.Id}.{id}";
            if (!Tracked.TryGetValue(cacheId, out var root))
            {
                root = await GetUnTracked(_parent.Bucket, id, _parent).ConfigureAwait(false);
                if (!Tracked.TryAdd(cacheId, root))
                    throw new InvalidOperationException($"Could not add cache key [{cacheId}] to repo tracked");
            }

            return root;
        }

        public override async Task<TEntity> New(Id id)
        {
            var cacheId = $"{_parent.Bucket}.{_parent.Id}.{id}";

            if (!Tracked.TryGetValue(cacheId, out var root))
            {
                root = await NewUnTracked(_parent.Bucket, id, _parent).ConfigureAwait(false);
                if (!Tracked.TryAdd(cacheId, root))
                    throw new InvalidOperationException($"Could not add cache key [{cacheId}] to repo tracked");
            }

            return root;
        }

        protected override async Task<TEntity> GetUnTracked(string bucket, Id id, IEntity parent)
        {
            var entity = await base.GetUnTracked(bucket, id, parent).ConfigureAwait(false);

            entity.Parent = _parent;
            (entity.State as State<TState, TStateParent>).Parent = _parent.State;

            return entity;
        }

        protected override async Task<TEntity> NewUnTracked(string bucket, Id id, IEntity parent)
        {
            var entity = await base.NewUnTracked(bucket, id, parent).ConfigureAwait(false);

            entity.Parent = _parent;

            return entity;
        }


    }
    public class Repository<TEntity, TState> : IRepository<TEntity>, IRepositoryCommit where TEntity : Entity<TEntity, TState> where TState : State<TState>, new()
    {
        private static readonly ILog Logger = LogProvider.GetLogger("Repository");


        protected readonly ConcurrentDictionary<string, TEntity> Tracked = new ConcurrentDictionary<string, TEntity>();
        private readonly IStoreEntities _store;

        private bool _disposed;

        public int ChangedStreams => Tracked.Count(x => x.Value.Dirty);

        // Todo: too many operations on this class, make a "EntityWriter" contract which does event, oob, and snapshot writing
        public Repository(IStoreEntities store)
        {
            _store = store;
        }
        Task IRepositoryCommit.Prepare(Guid commitId)
        {
            Logger.DebugEvent("Prepare", "{EntityType} prepare {CommitId}", typeof(TEntity).FullName, commitId);
            // Verify streams we read but didn't change are still save version
            return
                Tracked.Values
                    .Where(x => !x.Dirty)
                    .ToArray()
                    .WhenAllAsync(x => _store.Verify<TEntity, TState>(x));
        }


        async Task IRepositoryCommit.Commit(Guid commitId, IDictionary<string, string> commitHeaders)
        {
            Logger.DebugEvent("Commit", "[{EntityType:l}] commit {CommitId}", typeof(TEntity).FullName, commitId);

            await Tracked.Values
                .ToArray()
                .Where(x => x.Dirty)
                .WhenAllAsync(tracked =>
                    _store.Commit<TEntity, TState>(tracked, commitId, commitHeaders)
                ).ConfigureAwait(false);


            Logger.DebugEvent("FinishedCommit", "[{EntityType:l}] commit {CommitId}", typeof(TEntity).FullName, commitId);
        }


        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed || !disposing)
                return;

            Tracked.Clear();

            _disposed = true;
        }

        public virtual Task<TEntity> TryGet(Id id)
        {
            return TryGet(Defaults.Bucket, id);
        }
        public async Task<TEntity> TryGet(string bucket, Id id)
        {
            if (id == null) return default(TEntity);

            try
            {
                return await Get(bucket, id).ConfigureAwait(false);
            }
            catch (NotFoundException) { }
            return default(TEntity);
        }

        public virtual Task<TEntity> Get(Id id)
        {
            return Get(Defaults.Bucket, id);
        }

        public async Task<TEntity> Get(string bucket, Id id)
        {
            var cacheId = $"{bucket}.{id}";
            if (!Tracked.TryGetValue(cacheId, out var root))
            {
                root = await GetUnTracked(bucket, id).ConfigureAwait(false);
                if (!Tracked.TryAdd(cacheId, root))
                    throw new InvalidOperationException($"Could not add cache key [{cacheId}] to repo tracked");
            }

            return root;
        }
        protected virtual Task<TEntity> GetUnTracked(string bucket, Id id, IEntity parent = null)
        {
            return _store.Get<TEntity, TState>(bucket, id, parent);
        }


        public virtual Task<TEntity> New(Id id)
        {
            return New(Defaults.Bucket, id);
        }

        public async Task<TEntity> New(string bucket, Id id)
        {
            var cacheId = $"{bucket}.{id}";
            if (!Tracked.TryGetValue(cacheId, out var root))
            {
                root = await NewUnTracked(bucket, id).ConfigureAwait(false);
                if (!Tracked.TryAdd(cacheId, root))
                    throw new InvalidOperationException($"Could not add cache key [{cacheId}] to repo tracked");
            }
            return root;
        }
        protected virtual Task<TEntity> NewUnTracked(string bucket, Id id, IEntity parent = null)
        {
            return _store.New<TEntity, TState>(bucket, id, parent);
        }

    }
}
