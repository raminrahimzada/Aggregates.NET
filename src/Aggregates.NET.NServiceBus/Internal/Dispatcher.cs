using Aggregates.Contracts;
using Aggregates.Extensions;
using Aggregates.Logging;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Transport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aggregates.Internal
{
    class Dispatcher : IMessageDispatcher
    {
        private static readonly ILog Logger = LogProvider.GetLogger("Dispatcher");
        private readonly IMetrics _metrics;
        private readonly IMessageSerializer _serializer;
        private readonly IEventMapper _mapper;

        private static readonly BlockingCollection<Tuple<IFullMessage[], IDictionary<string, string>>> WaitingMessages = new BlockingCollection<Tuple<IFullMessage[], IDictionary<string, string>>>();
        private static Thread Delivery = new Thread((_) =>
        {
            while (!Bus.BusOnline)
                Thread.Sleep(100);

            var metrics = Configuration.Settings.Container.Resolve<IMetrics>();
            var mapper = Configuration.Settings.Container.Resolve<IEventMapper>();
            var serializer = Configuration.Settings.Container.Resolve<IMessageSerializer>();

            try
            {
                while (true)
                {
                    var messages = WaitingMessages.Take();

                    var headers = messages.Item2 ?? new Dictionary<string, string>();

                    var contextBag = new ContextBag();
                    if (messages.Item1.Length == 1)
                    {
                        // Hack to get all the events to invoker without NSB deserializing 
                        contextBag.Set(Defaults.LocalHeader, messages.Item1[0].Message);
                        headers = headers.Merge(messages.Item1[0].Headers);
                    }
                    else
                        // Hack to get all the events to invoker without NSB deserializing 
                        contextBag.Set(Defaults.BulkHeader, messages.Item1);

                    var processed = false;
                    var numberOfDeliveryAttempts = 0;


                    // All messages will be same type
                    var messageType = messages.Item1[0].Message.GetType();
                    if (!messageType.IsInterface)
                        messageType = mapper.GetMappedTypeFor(messageType) ?? messageType;

                    var finalHeaders = headers.Merge(new Dictionary<string, string>()
                    {
                        [Headers.EnclosedMessageTypes] = messageType.AssemblyQualifiedName,
                        [Headers.MessageIntent] = MessageIntentEnum.Send.ToString(),
                    });


                    var messageId = Guid.NewGuid().ToString();
                    var corrId = "";
                    if (finalHeaders.ContainsKey($"{Defaults.PrefixHeader}.{Defaults.MessageIdHeader}"))
                        messageId = finalHeaders[$"{Defaults.PrefixHeader}.{Defaults.MessageIdHeader}"];
                    if (finalHeaders.ContainsKey($"{Defaults.PrefixHeader}.{Defaults.CorrelationIdHeader}"))
                        corrId = finalHeaders[$"{Defaults.PrefixHeader}.{Defaults.CorrelationIdHeader}"];


                    finalHeaders[Headers.MessageId] = messageId;
                    finalHeaders[Headers.CorrelationId] = corrId;

                    while (!processed)
                    {
                        var transportTransaction = new TransportTransaction();
                        var tokenSource = new CancellationTokenSource();


                        try
                        {
                            var messageContext = new MessageContext(messageId,
                                finalHeaders,
                                Marker, transportTransaction, tokenSource,
                                contextBag);
                            Bus.OnMessage(messageContext).ConfigureAwait(false)
                                .GetAwaiter().GetResult();
                            metrics.Mark("Dispatched Messages", Unit.Message);
                            processed = true;
                        }
                        catch (ObjectDisposedException)
                        {
                            // NSB transport has been disconnected
                            throw new OperationCanceledException();
                        }
                        catch (Exception ex)
                        {
                            metrics.Mark("Dispatched Errors", Unit.Errors);

                            ++numberOfDeliveryAttempts;

                            // Don't retry a cancelation
                            if (tokenSource.IsCancellationRequested)
                                numberOfDeliveryAttempts = Int32.MaxValue;

                            var messageList = messages.Item1.ToList();
                            foreach (var message in messages.Item1)
                            {
                                var messageBytes = serializer.Serialize(message.Message);
                                var errorContext = new ErrorContext(ex, message.Headers.Merge(finalHeaders),
                                    messageId,
                                    messageBytes, transportTransaction,
                                    numberOfDeliveryAttempts);

                                var errorHandled = Bus.OnError(errorContext).ConfigureAwait(false)
                                                    .GetAwaiter().GetResult();

                                if (errorHandled == ErrorHandleResult.Handled || tokenSource.IsCancellationRequested)
                                    messageList.Remove(message);
                            }
                            if (messageList.Count == 0)
                                break;
                            messages = Tuple.Create(messageList.ToArray(), messages.Item2);

                        }
                    }
                }
            }

            catch (Exception e)
            {
                if (!(e is OperationCanceledException))
                    Logger.ErrorEvent("Died", e, "Event thread closed: {ExceptionType} - {ExceptionMessage}", e.GetType().Name, e.Message);
            }

        })
        { Name = "SendLocal Delivery", IsBackground = true };

        // A fake message that will travel through the pipeline in order to process events from the context bag
        private static readonly byte[] Marker = new byte[] { 0x7b, 0x7d };

        public Dispatcher(IMetrics metrics, IMessageSerializer serializer, IEventMapper mapper)
        {
            _metrics = metrics;
            _serializer = serializer;
            _mapper = mapper;
        }

        public Task Publish(IFullMessage[] messages)
        {
            var options = new PublishOptions();
            _metrics.Mark("Dispatched Messages", Unit.Message);

            // Todo: publish would only be called for messages on a single stream
            // we can set a routing key somehow for BulkMessage so its routed to the same sharded queue 
            var message = new BulkMessage
            {
                Messages = messages
            };
            // Publishing an IMessage normally creates a warning
            options.DoNotEnforceBestPractices();

            return Bus.Instance.Publish(message, options);
        }

        public Task Send(IFullMessage[] messages, string destination)
        {
            var options = new SendOptions();
            options.SetDestination(destination);

            var message = new BulkMessage
            {
                Messages = messages
            };

            _metrics.Mark("Dispatched Messages", Unit.Message);
            return Bus.Instance.Send(message, options);
        }


        public Task SendLocal(IFullMessage message, IDictionary<string, string> headers = null)
        {
            WaitingMessages.Add(Tuple.Create(new[] { message }, headers));
            return Task.CompletedTask;
        }

        public Task SendLocal(IFullMessage[] messages, IDictionary<string, string> headers = null)
        {
            foreach (var group in messages.GroupBy(x => x.Message.GetType()))
            {
                WaitingMessages.Add(Tuple.Create(group.ToArray(), headers));
            }
            return Task.CompletedTask;
        }
    }
}
