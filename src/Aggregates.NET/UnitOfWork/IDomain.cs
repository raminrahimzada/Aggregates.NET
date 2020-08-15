﻿using Aggregates.Contracts;
using System;
using System.Collections.Generic;

namespace Aggregates.UnitOfWork
{
    public interface IDomain : IMutate
    {
        IRepository<T> For<T>() where T : IEntity;
        IRepository<TEntity, TParent> For<TEntity, TParent>(TParent parent) where TEntity : IChildEntity<TParent> where TParent : IHaveEntities<TParent>;

        Guid CommitId { get; }
        object CurrentMessage { get; }
        IDictionary<string, string> CurrentHeaders { get; }
    }
}
