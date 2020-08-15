﻿using Aggregates.Attributes;
using Aggregates.Contracts;
using Aggregates.Exceptions;
using Aggregates.Extensions;
using Aggregates.Logging;
using Aggregates.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aggregates.Internal
{
    public class StoreEntities : IStoreEntities
    {
        private static readonly ILog Logger = LogProvider.GetLogger("StoreEntities");

        private readonly IMetrics _metrics;
        private readonly IStoreEvents _eventStore;
        private readonly IStoreSnapshots _snapStore;
        private readonly IOobWriter _oobStore;
        private readonly IEventFactory _factory;
        private readonly IVersionRegistrar _registrar;
        private readonly ITrackChildren _childTracker;

        public StoreEntities(IMetrics metrics, IStoreEvents eventStore, IStoreSnapshots snapStore, IOobWriter oobStore, IEventFactory factory, IVersionRegistrar registrar, ITrackChildren childTracker)
        {
            _metrics = metrics;
            _eventStore = eventStore;
            _snapStore = snapStore;
            _oobStore = oobStore;
            _factory = factory;
            _registrar = registrar;
            _childTracker = childTracker;
        }

        private IParentDescriptor[] getParents(IEntity entity)
        {
            if (entity == null)
                return null;
            
            var parents = getParents((entity as IChildEntity)?.Parent)?.ToList() ?? new List<IParentDescriptor>();
            parents.Add(new ParentDescriptor { EntityType = _registrar.GetVersionedName(entity.GetType()), StreamId = entity.Id });
            return parents.ToArray();
        }
        public Task<TEntity> New<TEntity, TState>(string bucket, Id id, IEntity parent) where TEntity : IEntity<TState> where TState : class, IState, new()
        {
            var uow = (Configuration.Settings.LocalContainer.Value ?? Configuration.Settings.Container).Resolve<Aggregates.UnitOfWork.IDomain>();

            var factory = EntityFactory.For<TEntity>();

            Logger.DebugEvent("Create", "[{EntityId:l}] bucket [{Bucket:l}] entity [{EntityType:l}]", id, bucket, typeof(TEntity).FullName);

            var entity = factory.Create(bucket, id, getParents(parent));

            ((INeedDomainUow) entity).Uow = uow;
            ((INeedEventFactory) entity).EventFactory = _factory;
            ((INeedStore) entity).Store = _eventStore;
            ((INeedStore) entity).OobWriter = _oobStore;
            ((INeedVersionRegistrar) entity).Registrar = _registrar;
            ((INeedChildTracking) entity).Tracker = _childTracker;

            return Task.FromResult(entity);
        }
        public async Task<TEntity> Get<TEntity, TState>(string bucket, Id id, IEntity parent) where TEntity : IEntity<TState> where TState : class, IState, new()
        {
            var uow = (Configuration.Settings.LocalContainer.Value ?? Configuration.Settings.Container).Resolve<Aggregates.UnitOfWork.IDomain>();

            var factory = EntityFactory.For<TEntity>();

            var parents = getParents(parent);
            // Todo: pass parent instead of Id[]?
            var snapshot = await _snapStore.GetSnapshot<TEntity>(bucket, id, parents?.Select(x => x.StreamId).ToArray()).ConfigureAwait(false);
            var events = await _eventStore.GetEvents<TEntity>(bucket, id, parents?.Select(x => x.StreamId).ToArray(), start: snapshot?.Version).ConfigureAwait(false);

            var entity = factory.Create(bucket, id, parents, events.Select(x => x.Event as IEvent).ToArray(), snapshot?.Payload);


            ((INeedDomainUow) entity).Uow = uow;
            ((INeedEventFactory) entity).EventFactory = _factory;
            ((INeedStore) entity).Store = _eventStore;
            ((INeedStore) entity).OobWriter = _oobStore;
            ((INeedVersionRegistrar) entity).Registrar = _registrar;
            ((INeedChildTracking) entity).Tracker = _childTracker;

            Logger.DebugEvent("Get", "[{EntityId:l}] bucket [{Bucket:l}] entity [{EntityType:l}] version {Version}", id, bucket, typeof(TEntity).FullName, entity.Version);

            return entity;
        }
        public Task Verify<TEntity, TState>(TEntity entity) where TEntity : IEntity<TState> where TState : class, IState, new()
        {
            if (entity.Dirty)
                throw new ArgumentException($"Cannot verify version for a dirty entity");

            return _eventStore.VerifyVersion<TEntity>(entity.Bucket, entity.Id, entity.GetParentIds(), entity.Version);
        }
        public async Task Commit<TEntity, TState>(TEntity entity, Guid commitId, IDictionary<string, string> commitHeaders) where TEntity : IEntity<TState> where TState : class, IState, new()
        {
            if (!entity.Dirty)
                throw new ArgumentException($"Entity {typeof(TEntity).FullName} id {entity.Id} bucket {entity.Bucket} is not dirty");

            var state = entity.State;

            var domainEvents = entity.Uncommitted.Where(x => x.Descriptor.StreamType == StreamTypes.Domain).ToArray();
            var oobEvents = entity.Uncommitted.Where(x => x.Descriptor.StreamType == StreamTypes.OOB).ToArray();

            try
            {
                if (domainEvents.Any())
                {
                    await _eventStore.WriteEvents<TEntity>(entity.Bucket, entity.Id, entity.GetParentIds(),
                        domainEvents, commitHeaders, entity.Version).ConfigureAwait(false);
                }
            }
            catch (VersionException e)
            {
                Logger.DebugEvent("VersionConflict", "[{EntityId:l}] entity [{EntityType:l}] version {Version} commit version {CommitVersion} - {StoreMessage}", entity.Id, typeof(TEntity).FullName, state.Version, entity.Version, e.Message);
                _metrics.Mark("Conflicts", Unit.Items);
                // If we expected no stream, no reason to try to resolve the conflict
                if (entity.Version == EntityFactory.NewEntityVersion)
                {
                    Logger.DebugEvent("AlreadyExists", "[{EntityId:l}] entity [{EntityType:l}] already exists", entity.Id, typeof(TEntity).FullName);
                    throw new EntityAlreadyExistsException<TEntity>(entity.Bucket, entity.Id, entity.GetParentIds());
                }

                try
                {
                    // Todo: cache per entity type
                    var conflictResolution = (OptimisticConcurrencyAttribute)Attribute.GetCustomAttribute(typeof(TEntity), typeof(OptimisticConcurrencyAttribute))
                                          ?? new OptimisticConcurrencyAttribute(ConcurrencyConflict.Throw);

                    Logger.DebugEvent("ConflictResolve", "[{EntityId:l}] entity [{EntityType:l}] resolving {ConflictingEvents} events with {ConflictResolver}", entity.Id, typeof(TEntity).FullName, entity.Uncommitted.Count(), conflictResolution.Conflict);
                    var strategy = conflictResolution.Conflict.Build(conflictResolution.Resolver);

                    commitHeaders[Defaults.ConflictResolvedHeader] = conflictResolution.Conflict.DisplayName;

                    await strategy.Resolve<TEntity, TState>(entity, commitId, commitHeaders).ConfigureAwait(false);

                    Logger.DebugEvent("ConflictResolveSuccess", "[{EntityId:l}] entity [{EntityType:l}] resolution success", entity.Id, typeof(TEntity).FullName);
                }
                catch (AbandonConflictException abandon)
                {
                    _metrics.Mark("Conflicts Unresolved", Unit.Items);
                    Logger.ErrorEvent("ConflictResolveAbandon", "[{EntityId:l}] entity [{EntityType:l}] abandonded", entity.Id, typeof(TEntity).FullName);

                    throw new ConflictResolutionFailedException(entity.GetType(), entity.Bucket, entity.Id, entity.GetParentIds(), "Aborted", abandon);
                }
                catch (Exception ex)
                {
                    _metrics.Mark("Conflicts Unresolved", Unit.Items);
                    Logger.ErrorEvent("ConflictResolveFail", ex, "[{EntityId:l}] entity [{EntityType:l}] failed: {ExceptionType} - {ExceptionMessage}", entity.Id, typeof(TEntity).FullName, ex.GetType().Name, ex.Message);

                    throw new ConflictResolutionFailedException(entity.GetType(), entity.Bucket, entity.Id, entity.GetParentIds(), "Exception", ex);
                }

            }
            catch (PersistenceException e)
            {
                Logger.WarnEvent("CommitFailure", e, "[{EntityId:l}] entity [{EntityType:l}] bucket [{Bucket:l}]: {ExceptionType} - {ExceptionMessage}", entity.Id, typeof(TEntity).Name, entity.Bucket, e.GetType().Name, e.Message);
                _metrics.Mark("Event Write Errors", Unit.Errors);
                throw;
            }

            try
            {
                if (oobEvents.Any())
                    await _oobStore.WriteEvents<TEntity>(entity.Bucket, entity.Id, entity.GetParentIds(), oobEvents, commitId, commitHeaders).ConfigureAwait(false);

                if (entity.State.ShouldSnapshot())
                {
                    // Notify the entity and state that we are taking a snapshot
                    (entity as IEntity<TState>).Snapshotting();
                    entity.State.Snapshotting();
                    await _snapStore.WriteSnapshots<TEntity>(entity.State, commitHeaders).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger.WarnEvent("SecondaryFailure", "[{EntityId:l}] entity [{EntityType:l}] bucket [{Bucket:l}]: {ExceptionType} - {ExceptionMessage}", entity.Id, typeof(TEntity).Name, entity.Bucket, e.GetType().Name, e.Message);
            }
        }
    }
}
