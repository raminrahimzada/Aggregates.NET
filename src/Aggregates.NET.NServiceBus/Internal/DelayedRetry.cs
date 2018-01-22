using System;
using System.Collections.Generic;
using System.Text;
using Aggregates.Contracts;
using NServiceBus;

namespace Aggregates.Internal
{
    class DelayedRetry
    {
        private readonly IMetrics _metrics;
        private readonly IMessageDispatcher _dispatcher;

        public DelayedRetry(IMetrics metrics, IMessageDispatcher dispatcher)
        {
            _metrics = metrics;
            _dispatcher = dispatcher;
        }

        public void QueueRetry(IFullMessage message, TimeSpan delay)
        {
            _metrics.Increment("Retry Queue", Unit.Message);
            var messageId = Guid.NewGuid().ToString();
            message.Headers.TryGetValue(Headers.MessageId, out messageId);

            Timer.Expire(() =>
            {
                _metrics.Decrement("Retry Queue", Unit.Message);
                return _dispatcher.SendLocal(message);
            }, delay, $"message {messageId}");
        }        
    }
}
