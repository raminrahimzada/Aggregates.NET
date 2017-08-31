using System;
using System.Collections.Generic;
using System.Text;
using Aggregates.Messages;

namespace Aggregates.Contracts
{
    interface IEntityFactory<TEntity> where TEntity : IEntity
    {
        TEntity CreateNew(Id id);
        TEntity CreateFromEvents(Id id, IFullEvent[] events, IState snapshot = null);
    }
}
