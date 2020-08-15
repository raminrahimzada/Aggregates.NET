using Aggregates.Extensions;
using System;

namespace Aggregates.Exceptions
{
    public class EntityAlreadyExistsException<TEntity> : Exception
    {
        public EntityAlreadyExistsException(string bucket, Id id, Id[] parents) :
            base($"New stream [{id}] bucket [{bucket}] parents [{parents.BuildParentsString()}] entity {typeof(TEntity).FullName} already exists in store")
        { }
    }
}
