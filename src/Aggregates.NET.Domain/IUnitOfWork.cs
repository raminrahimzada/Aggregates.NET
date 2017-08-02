using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aggregates.Contracts;
using NServiceBus.ObjectBuilder;

namespace Aggregates
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> For<TEntity>() where TEntity : IEventSource;
        IRepository<TEntity, TParent> For<TEntity, TParent>(TParent parent) where TEntity : IEventSource<TParent> where TParent : IEventSource;
        IPocoRepository<T> Poco<T>() where T : class, new();
        IPocoRepository<T, TParent> Poco<T, TParent>(TParent parent) where T : class, new() where TParent : IEventSource;


        Task<IEnumerable<TResponse>> Query<TQuery, TResponse>(TQuery query) where TResponse : IQueryResponse where TQuery : IQuery<TResponse>;
        Task<IEnumerable<TResponse>> Query<TQuery, TResponse>(Action<TQuery> query) where TResponse : IQueryResponse where TQuery : IQuery<TResponse>;

        Task<TResponse> Compute<TComputed, TResponse>(TComputed computed) where TComputed : IComputed<TResponse>;
        Task<TResponse> Compute<TComputed, TResponse>(Action<TComputed> computed) where TComputed : IComputed<TResponse>;
        
        Guid CommitId { get; }
        object CurrentMessage { get; }
        IDictionary<string, string> CurrentHeaders { get; }
    }
}