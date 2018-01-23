using Aggregates.Contracts;
using Aggregates.Extensions;
using Aggregates.Internal.Cloning;
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
    class ExceptionRejector : Behavior<IIncomingLogicalMessageContext>
    {
        private static readonly ConcurrentDictionary<string, int> RetryRegistry = new ConcurrentDictionary<string, int>();
        private static readonly ILog Logger = LogProvider.GetLogger("ExceptionRejector");
        
        private readonly IMetrics _metrics;
        private readonly int _retries;
        private readonly DelayedRetry _retry;
        private readonly IMessageSerializer _serializer;

        public ExceptionRejector(IMetrics metrics, int retries, DelayedRetry retry, IMessageSerializer serializer)
        {
            _metrics = metrics;
            _retries = retries;
            _retry = retry;
            _serializer = serializer;
        }

        public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            var messageId = context.MessageId;
            var retries = 0;

            // Make a copy of the message to send for retries if needed
            // since downstream Bulk unpack and others will modify instance and header data
            var message = new FullMessage
            {
                Message = context.Message.Instance,
                Headers = context.Headers.ToDictionary(kv => kv.Key, kv => kv.Value)
            };
            context.MessageHeaders

            try
            {
                RetryRegistry.TryRemove(messageId, out retries);
                context.Headers[Defaults.Retries] = retries.ToString();
                context.Extensions.Set(Defaults.Retries, retries);

                await next().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Special exception we dont want to retry or reply
                if (e is BusinessException || context.MessageHandled)
                    return;

                var stackTrace = string.Join("\n", (e.StackTrace?.Split('\n').Take(10) ?? new string[] { }).AsEnumerable());

                if (retries < _retries || _retries == -1)
                {
                    Logger.LogEvent((retries > _retries / 2) ? LogLevel.Warn : LogLevel.Info, "Catch", e, "[{MessageId:l}] will retry {Retries}/{MaxRetries}: {ExceptionType} - {ExceptionMessage}", messageId,
                        retries, _retries, e.GetType().Name, e.Message);
                    
                    RetryRegistry.TryAdd(messageId, retries + 1);                    
                    _retry.QueueRetry(message, TimeSpan.FromMilliseconds(500));
                    // retry out of the pipeline so NSB can continue processing other messages & we can delay
                    //throw;
                    return;
                }

                // at this point the message has failed, so a THROW will move it to the error queue
                
                // Only send reply if the message is a SEND, else we risk endless reply loops as message failures bounce back and forth
                if (context.GetMessageIntent() != MessageIntentEnum.Send && context.GetMessageIntent() != MessageIntentEnum.Publish)
                    return;

                // At this point message is dead - should be moved to error queue, send message to client that their request was rejected due to error 
                _metrics.Mark("Message Faults", Unit.Errors);
                
                Logger.ErrorEvent("Fault", e, "[{MessageId:l}] has failed {Retries} times\n{@Headers}\n{Body}\n{ExceptionType} - {ExceptionMessage}", messageId, retries, message.Headers, _serializer.Serialize(message.Message).AsString().MaxLines(20), e.GetType().Name, e.Message);
                // Only need to reply if the client expects it
                if (!message.Headers.ContainsKey(Defaults.RequestResponse) ||
                    message.Headers[Defaults.RequestResponse] != "1")
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
    internal class ExceptionRejectorRegistration : RegisterStep
    {
        public ExceptionRejectorRegistration(IContainer container) : base(
            stepId: "ExceptionRejector",
            behavior: typeof(ExceptionRejector),
            description: "handles exceptions and retries",
            factoryMethod: (b) => new ExceptionRejector(container.Resolve<IMetrics>(), Configuration.Settings.Retries, container.Resolve<DelayedRetry>(), container.Resolve<IMessageSerializer>())
        )
        {
            InsertBefore("MutateIncomingMessages");
        }
    }
}
