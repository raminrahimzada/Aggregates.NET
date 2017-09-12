using Aggregates.Contracts;
using Aggregates.DI;
using Aggregates.Extensions;
using Aggregates.Internal;
using Aggregates.Logging;
using Aggregates.Messages;
using NServiceBus;
using NServiceBus.Features;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates
{
    class Feature : NServiceBus.Features.Feature
    {
        public Feature()
        {
            DependsOn("NServiceBus.Features.ReceiveFeature");
            Defaults(s =>
            {
                s.SetDefault("Retries", 10);
                s.SetDefault("ReadSize", 200);
                s.SetDefault("Compress", Compression.Snapshots);
                s.SetDefault("SlowAlertThreshold", 1000);
                s.SetDefault("SlowAlerts", false);
                s.SetDefault("MaxDelayed", 10000);
                s.SetDefault("StreamGenerator", new StreamIdGenerator((type, streamType, bucket, stream, parents) => $"{streamType}-{bucket}-[{parents.BuildParentsString()}]-{type.FullName.Replace(".", "")}-{stream}"));
            });
        }
        protected override void Setup(FeatureConfigurationContext context)
        {
            var container = TinyIoCContainer.Current;

            container.Register<IEventFactory, EventFactory>();
            container.Register<IMessageDispatcher, Dispatcher>();
            container.Register<IEventMapper, EventMapper>();
            container.Register<IMessaging, NServiceBusMessaging>();

            container.Register<IDomainUnitOfWork, NSBUnitOfWork>();
            MutationManager.RegisterMutator("domain unit of work", typeof(NSBUnitOfWork));

            context.RegisterStartupTask(builder => new EndpointRunner(context.Settings.InstanceSpecificQueue()));

            var settings = context.Settings;
            context.Pipeline.Register(
                b => new ExceptionRejector(TinyIoCContainer.Current.Resolve<IMetrics>(), settings.Get<int>("Retries")),
                "Watches message faults, sends error replies to client when message moves to error queue"
                );

            if (settings.Get<bool>("SlowAlerts"))
                context.Pipeline.Register(
                    behavior: new TimeExecutionBehavior(settings.Get<int>("SlowAlertThreshold")),
                    description: "times the execution of messages and reports anytime they are slow"
                    );

            context.Pipeline.Register<UowRegistration>();
            context.Pipeline.Register<MutateIncomingRegistration>();
            context.Pipeline.Register<MutateOutgoingRegistration>();

            // We are sending IEvents, which NSB doesn't like out of the box - so turn that check off
            context.Pipeline.Remove("EnforceSendBestPractices");

        }
    }

    class EndpointRunner : FeatureStartupTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger("EndpointRunner");
        private readonly String _instanceQueue;

        public EndpointRunner(String instanceQueue)
        {
            _instanceQueue = instanceQueue;
        }
        protected override async Task OnStart(IMessageSession session)
        {
            Logger.Write(LogLevel.Info, "Starting endpoint");

            await session.Publish<EndpointAlive>(x =>
            {
                x.Endpoint = _instanceQueue;
                x.Instance = Defaults.Instance;
            }).ConfigureAwait(false);

        }
        protected override async Task OnStop(IMessageSession session)
        {
            Logger.Write(LogLevel.Info, "Stopping endpoint");
            await session.Publish<EndpointDead>(x =>
            {
                x.Endpoint = _instanceQueue;
                x.Instance = Defaults.Instance;
            }).ConfigureAwait(false);
        }
    }
}
