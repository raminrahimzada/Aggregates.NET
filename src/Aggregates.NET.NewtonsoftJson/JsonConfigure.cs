﻿using Newtonsoft.Json;
using Aggregates.Contracts;
using Aggregates.Internal;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Aggregates
{
    [ExcludeFromCodeCoverage]
    public static class JsonConfigure
    {
        public static Configure NewtonsoftJson(this Configure config, JsonConverter[] extraConverters = null)
        {
            extraConverters = extraConverters ?? new JsonConverter[] { };

            config.MessageContentType = "json";
            config.RegistrationTasks.Add(c =>
            {
                var container = c.Container;
                                
                container.Register<IMessageSerializer>(factory => new JsonMessageSerializer(factory.Resolve<IEventMapper>(), factory.Resolve<IEventFactory>(), extraConverters), Lifestyle.Singleton);

                return Task.CompletedTask;
            });
            return config;
        }
    }
}
