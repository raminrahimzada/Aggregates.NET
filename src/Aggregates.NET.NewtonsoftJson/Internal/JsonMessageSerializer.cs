﻿using Aggregates.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using NewtonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Aggregates.Internal
{
    // https://github.com/Particular/NServiceBus.Newtonsoft.Json/blob/develop/src/NServiceBus.Newtonsoft.Json/JsonMessageSerializer.cs
    [ExcludeFromCodeCoverage]
    internal class JsonMessageSerializer : IMessageSerializer
    {
        public static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private readonly IEventMapper _messageMapper;
        //private IEventFactory _messageFactory;
        private readonly Func<Stream, JsonReader> _readerCreator;
        private readonly Func<Stream, JsonWriter> _writerCreator;
        private readonly NewtonSerializer _jsonSerializer;

        public JsonMessageSerializer(
            IEventMapper messageMapper,
            IEventFactory messageFactory,
            JsonConverter[] extraConverters)
        {
            _messageMapper = messageMapper;
            //this._messageFactory = messageFactory;

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new JsonConverter[] { new Newtonsoft.Json.Converters.StringEnumConverter(), new IdJsonConverter() }.Concat(extraConverters).ToArray(),
                //Error = new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>(HandleError),
                ContractResolver = new EventContractResolver(messageMapper, messageFactory),
                SerializationBinder = new EventSerializationBinder(messageMapper),
                //TraceWriter = new TraceWriter(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include
            };

            _writerCreator = (stream =>
            {
                var streamWriter = new StreamWriter(stream, Utf8NoBom);
                return new JsonTextWriter(streamWriter)
                {
                    // better for displaying
                    Formatting = Formatting.Indented
                };
            });

            _readerCreator = (stream =>
            {
                var streamReader = new StreamReader(stream, Utf8NoBom);
                return new JsonTextReader(streamReader);
            });

            ContentType = "Json";
            _jsonSerializer = NewtonSerializer.Create(settings);
        }

        //private static void HandleError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        //{
        //    args.ErrorContext.Handled = true;
        //    throw new SerializerException(args.ErrorContext.Error, args.ErrorContext.Path);
        //}

        public void Serialize(object message, Stream stream)
        {
            using (var writer = _writerCreator(stream))
            {
                writer.CloseOutput = false;
                _jsonSerializer.Serialize(writer, message);
                writer.Flush();
            }
        }

        public object[] Deserialize(Stream stream, IList<Type> messageTypes)
        {
            var isArrayStream = IsArrayStream(stream);

            if (messageTypes.Any())
            {
                return DeserializeMultipleMessageTypes(stream, messageTypes, isArrayStream);
            }

            return new[]
            {
                ReadObject(stream, isArrayStream, typeof(object))
            };
        }

        private object ReadObject(Stream stream, bool isArrayStream, Type type)
        {
            using (var reader = _readerCreator(stream))
            {
                reader.CloseInput = false;
                if (isArrayStream)
                {
                    var objects = (object[])_jsonSerializer.Deserialize(reader, type.MakeArrayType());
                    if (objects.Length > 1)
                    {
                        throw new Exception("Multiple messages in the same stream are not supported.");
                    }
                    return objects[0];
                }

                return _jsonSerializer.Deserialize(reader, type);
            }
        }

        public string ContentType { get; }

        private object[] DeserializeMultipleMessageTypes(Stream stream, IList<Type> messageTypes, bool isArrayStream)
        {
            var rootTypes = FindRootTypes(messageTypes).ToList();
            var messages = new object[rootTypes.Count];
            for (var index = 0; index < rootTypes.Count; index++)
            {
                var messageType = rootTypes[index];
                stream.Seek(0, SeekOrigin.Begin);

                messageType = GetMappedType(messageType);
                messages[index] = ReadObject(stream, isArrayStream, messageType);
            }
            return messages;
        }

        private Type GetMappedType(Type messageType)
        {
            if (messageType.IsInterface)
            {
                var mappedTypeFor = _messageMapper.GetMappedTypeFor(messageType);
                if (mappedTypeFor != null)
                {
                    return mappedTypeFor;
                }
            }
            return messageType;
        }

        private bool IsArrayStream(Stream stream)
        {
            using (var reader = _readerCreator(stream))
            {
                reader.CloseInput = false;
                reader.Read();
                stream.Seek(0, SeekOrigin.Begin);
                return reader.TokenType == JsonToken.StartArray;
            }
        }

        private static IEnumerable<Type> FindRootTypes(IEnumerable<Type> messageTypesToDeserialize)
        {
            Type currentRoot = null;
            foreach (var type in messageTypesToDeserialize)
            {
                if (currentRoot == null)
                {
                    currentRoot = type;
                    yield return currentRoot;
                    continue;
                }
                if (!type.IsAssignableFrom(currentRoot))
                {
                    currentRoot = type;
                    yield return currentRoot;
                }
            }
        }
    }
}
