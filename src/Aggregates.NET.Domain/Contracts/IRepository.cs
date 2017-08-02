using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aggregates.Contracts
{
    interface IRepository : IDisposable
    {
        int TotalUncommitted { get; }
        int ChangedStreams { get; }

        // Checks stream versions in store if needed
        Task Prepare(Guid commitId);
        // Writes the stream
        Task Commit(Guid commitId, IDictionary<string, string> commitHeaders);
    }

    public interface IRepository<TEntity> where TEntity : IEventSource
    {
        /// <summary>
        /// Attempts to get aggregate from store, if stream does not exist it throws
        /// </summary>
        Task<TEntity> Get(Id id);
        Task<TEntity> Get(string bucketId, Id id);
        /// <summary>
        /// Attempts to retreive aggregate from store, if stream does not exist it does not throw
        /// </summary>
        Task<TEntity> TryGet(Id id);
        Task<TEntity> TryGet(string bucketId, Id id);

        /// <summary>
        /// Initiates a new event stream
        /// </summary>
        Task<TEntity> New(Id id);
        Task<TEntity> New(string bucketId, Id id);
    }
    public interface IRepository<TEntity, TParent> where TParent : IEventSource where TEntity : IEventSource 
    {
        /// <summary>
        /// Attempts to get aggregate from store, if stream does not exist it throws
        /// </summary>
        Task<TEntity> Get(Id id);
        /// <summary>
        /// Attempts to retreive aggregate from store, if stream does not exist it does not throw
        /// </summary>
        Task<TEntity> TryGet(Id id);
        /// <summary>
        /// Initiates a new event stream
        /// </summary>
        Task<TEntity> New(Id id);
    }
}