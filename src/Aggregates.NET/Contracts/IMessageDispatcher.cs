using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aggregates.Contracts
{
    public interface IMessageDispatcher
    {
        Task SendLocal(IFullMessage message, IDictionary<string, string> headers = null);
        Task SendLocal(IFullMessage[] messages, IDictionary<string, string> headers = null);
        Task Send(IFullMessage[] message, string destination);
        Task Publish(IFullMessage[] message);
        Task SendToError(Exception ex, IFullMessage message);
    }
}
