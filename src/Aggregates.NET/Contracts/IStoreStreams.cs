using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus.ObjectBuilder;

namespace Aggregates.Contracts
{
    public interface IStoreStreams
    {
        Task<IEventStream> GetStream<T>(string bucket, Id streamId, IEnumerable<Id> parents = null) where T : IEventSource;
        Task<IEventStream> NewStream<T>(string bucket, Id streamId, IEnumerable<Id> parents = null) where T : IEventSource;

        Task<long> GetSize<T>(IEventStream stream, string oob) where T : IEventSource;
        Task<IEnumerable<IFullEvent>> GetEvents<T>(IEventStream stream, long start, int count, string oob = null) where T : IEventSource;
        Task<IEnumerable<IFullEvent>> GetEventsBackwards<T>(IEventStream stream, long start , int count, string oob = null) where T : IEventSource;

        Task WriteStream<T>(Guid commitId, IEventStream stream, IDictionary<string, string> commitHeaders) where T : IEventSource;
        Task VerifyVersion<T>(IEventStream stream) where T : IEventSource;
        
    }
}