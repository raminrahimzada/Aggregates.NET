﻿using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Aggregates.Internal
{
    [ExcludeFromCodeCoverage]
    internal class IdJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Id) == objectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
                return new Id((long)reader.Value);

            var str = reader.Value as string;
            return Guid.TryParse(str, out var guid) ? new Id(guid) : new Id(str);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var id = (Id)value;
            writer.WriteValue(id.Value);
        }
    }
}
