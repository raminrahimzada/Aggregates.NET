﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Aggregates.Messages;

namespace Aggregates
{
    public interface IUnitOfWork
    {
        Task Begin();
        Task End(Exception ex = null);
    }

    public interface IDomainUnitOfWork
    {
        Task Begin();
        Task End(Exception ex = null);

        IRepository<T> For<T>() where T : class, IEntity;
        IRepository<TEntity, TParent> For<TEntity, TParent>(TParent parent) where TEntity : class, IChildEntity<TParent> where TParent : class, IEntity;
        IPocoRepository<T> Poco<T>() where T : class, new();
        IPocoRepository<T, TParent> Poco<T, TParent>(TParent parent) where T : class, new() where TParent : class, IEntity;


        Task<TResponse> Query<TQuery, TResponse>(TQuery query, IContainer container) where TQuery : class, IQuery<TResponse>;
        Task<TResponse> Query<TQuery, TResponse>(Action<TQuery> query, IContainer container) where TQuery : class, IQuery<TResponse>;

        Guid CommitId { get; }
        object CurrentMessage { get; }
        IDictionary<string, string> CurrentHeaders { get; }
    }
}
