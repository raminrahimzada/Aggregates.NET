using Aggregates.Contracts;
using Aggregates.DI;
using Aggregates.Internal;
using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aggregates
{
    public static class ESConfigure
    {
        public static Configure EventStore(this Configure config, IEventStoreConnection[] connections)
        {
            var container = TinyIoCContainer.Current;

            container.Register<IEventStoreConsumer>((factory, overloads) => 
                new EventStoreConsumer(
                    factory.Resolve<IMetrics>(), 
                    factory.Resolve<IMessageSerializer>(), 
                    connections, 
                    config.ReadSize, 
                    config.ExtraStats
                    )).AsMultiInstance();
            container.Register<IStoreEvents>((factory, overloads) => 
                new StoreEvents(
                    factory.Resolve<IMetrics>(),
                    factory.Resolve<IMessageSerializer>(),
                    factory.Resolve<IEventMapper>(),
                    config.Generator,
                    config.ReadSize,
                    config.Compression,
                    connections
                    )).AsMultiInstance();

            return config;
        }
    }
}
