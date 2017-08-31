using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Aggregates.NET.NewtonsoftJson
{
    public class WireUp
    {
        public static void Build()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                //SerializationBinder = new EventSerializationBinder(mapper),
                //ContractResolver = new EventContractResolver(mapper),
                Converters = new JsonConverter[] { new Newtonsoft.Json.Converters.StringEnumConverter(), new Internal.IdJsonConverter() },

            };

        }
    }
}
