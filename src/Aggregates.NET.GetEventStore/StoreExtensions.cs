﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Aggregates.Internal;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Aggregates.Extensions
{
    public static class StoreExtensions
    {
        public static byte[] AsByteArray(this String json)
        {
            return Encoding.UTF8.GetBytes(json);
        }

        public static String Serialize(this ISnapshot snapshot, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(snapshot, settings);
        }
        public static String Serialize(this Object @event, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(@event, settings);
        }

        public static String Serialize(this EventDescriptor descriptor, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(descriptor, settings);
        }

        public static Object Deserialize(this byte[] bytes, string type, JsonSerializerSettings settings)
        {
            var json = Encoding.UTF8.GetString(bytes);
            var resolved = Type.GetType(type, false);
            if (resolved == null) return null;

            return JsonConvert.DeserializeObject(json, resolved, settings);
        }

        public static EventDescriptor Deserialize(this byte[] bytes, JsonSerializerSettings settings)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<EventDescriptor>(json, settings);
        }

        public static T Deserialize<T>(this byte[] bytes, JsonSerializerSettings settings)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static String toLowerCamelCase(this String type)
        {
            // Unsure if I want to trim the namespaces or not
            var name = type.Substring(type.LastIndexOf('.') + 1);
            return char.ToLower(name[0]) + name.Substring(1);
        }

        public static T WaitForResult<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }

        public static void DispatchEvents(this IEventStoreConnection client, IDispatcher dispatcher, JsonSerializerSettings settings)
        {
            // Need message mapper


            // Idea is to subscribe to stream updates from event store in order to publish the events via NSB
            // Servers not needing a persistent stream can subscribe to these events (things like email notifications)
            client.SubscribeToAllFrom(Position.End, false, (s, e) =>
            {
                // Unsure if we need to care about events from eventstore currently
                if (!e.Event.IsJson) return;

                var descriptor = e.Event.Metadata.Deserialize(settings);

                if (descriptor == null) return;

                // Check if the event was written by this domain handler
                // We don't need to publish events saved by other domain instances
                Object header = null;
                Guid domain = Guid.Empty;
                if (!descriptor.Headers.TryGetValue(UnitOfWork.DomainHeader, out header) || !Guid.TryParse(header.ToString(), out domain) || domain != Domain.Current)
                    return;

                var data = e.Event.Data.Deserialize(e.Event.EventType, settings);
                
                if (data == null) return;

                var @event = new Internal.WritableEvent
                {
                    Descriptor = descriptor,
                    Event = data,
                    EventId = e.Event.EventId
                };

                dispatcher.Dispatch(@event);
            });
        }
    }
}