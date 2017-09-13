using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Aggregates.DI;
using Aggregates.Contracts;
using Aggregates.Internal;

namespace Aggregates
{
    public static class JsonConfigure
    {
        public static Configure NewtonsoftJson(this Configure config)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new JsonConverter[] { new Newtonsoft.Json.Converters.StringEnumConverter(), new Internal.IdJsonConverter() },
            };

            var container = TinyIoCContainer.Current;
            container.Register<IMessageSerializer>((factory, overloads) => new JsonMessageSerializer(factory.Resolve<IEventMapper>(), null, null, settings, null));

            return config;
        }
    }
}
