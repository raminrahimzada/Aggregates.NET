using Aggregates.Contracts;
using Aggregates.Extensions;
using Aggregates.Logging;
using Aggregates.Messages;
using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.Transport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.Internal
{
    class ExceptionRejector : Behavior<IIncomingPhysicalMessageContext>
    {
        private static readonly ConcurrentDictionary<string, int> RetryRegistry = new ConcurrentDictionary<string, int>();
        private static readonly ILog Logger = LogProvider.GetLogger("ExceptionRejector");
        
        private readonly IMetrics _metrics;
        private readonly int _retries;

        public ExceptionRejector(IMetrics metrics, int retries)
        {
            _metrics = metrics;
            _retries = retries;
        }

        public override async Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
        {
            var messageId = context.MessageId;
            var retries = 0;

            try
            {
                RetryRegistry.TryRemove(messageId, out retries);
                context.Message.Headers[Defaults.Retries] = retries.ToString();
                context.Extensions.Set(Defaults.Retries, retries);

                await next().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Special exception we dont want to retry or reply
                if (e is BusinessException)
                    return;

                var stackTrace = string.Join("\n", (e.StackTrace?.Split('\n').Take(10) ?? new string[] { }).AsEnumerable());

                if (retries < _retries || _retries == -1)
                {
                    Logger.LogEvent((retries > _retries / 2) ? LogLevel.Warn : LogLevel.Info, "Catch", e, "[{MessageId:l}] will retry {Retries}/{MaxRetries}: {ExceptionType} - {ExceptionMessage}", context.MessageId,
                        retries, _retries, e.GetType().Name, e.Message);
                    
                    RetryRegistry.TryAdd(messageId, retries + 1);
                    throw;
                }

                // Only send reply if the message is a SEND, else we risk endless reply loops as message failures bounce back and forth
                if (context.Message.GetMessageIntent() != MessageIntentEnum.Send && context.Message.GetMessageIntent() != MessageIntentEnum.Publish)
                    throw;

                // At this point message is dead - should be moved to error queue, send message to client that their request was rejected due to error 
                _metrics.Mark("Message Faults", Unit.Errors);
                
                Logger.ErrorEvent("Fault", e, "[{MessageId:l}] has failed {Retries}\n{@Headers}\n{Body}\n{ExceptionType} - {ExceptionMessage}", context.MessageId, retries, context.MessageHeaders, context.Message.Body.AsString().MaxLines(20), e.GetType().Name, e.Message);
                // Only need to reply if the client expects it
                if (!context.Message.Headers.ContainsKey(Defaults.RequestResponse) ||
                    context.Message.Headers[Defaults.RequestResponse] != "1")
                    throw;

                // Tell the sender the command was not handled due to a service exception
                var rejection = context.Builder.Build<Func<Exception, string, Error>>();
                // Wrap exception in our object which is serializable
                await context.Reply(rejection(e,
                            $"Rejected message after {retries} attempts!"))
                        .ConfigureAwait(false);

                // Should be the last throw for this message - if RecoveryPolicy is properly set the message will be sent over to error queue
                throw;

            }
        }
    }
}
