using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aggregates.Contracts
{
    public interface IEventSubscriber : IDisposable
    {
        Task Setup(string endpoint, CancellationToken cancelToken, Version version);

        Task Connect();
    }
}