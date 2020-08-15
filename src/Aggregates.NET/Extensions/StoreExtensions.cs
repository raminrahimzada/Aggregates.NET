﻿using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Aggregates.Contracts;
using System.Linq;

namespace Aggregates.Extensions
{
    internal static class StoreExtensions
    {
        public static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        
        public static byte[] AsByteArray(this string json)
        {
            return Utf8NoBom.GetBytes(json);
        }
        public static string AsString(this byte[] bytes)
        {
            return Utf8NoBom.GetString(bytes);
        }
        public static byte[] Compress(this byte[] bytes)
        {
            using (var stream = new MemoryStream())
            {
                using (var zip = new GZipStream(stream, CompressionMode.Compress))
                {
                    using (var writer = new BinaryWriter(zip, Utf8NoBom))
                    {
                        writer.Write(bytes.Length);
                        writer.Write(bytes, 0, bytes.Length);
                    }
                }
                return stream.ToArray();
            }
        }
        public static byte[] Decompress(this byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var gz = new GZipStream(stream, CompressionMode.Decompress))
                {
                    using (var reader = new BinaryReader(gz, Utf8NoBom))
                    {
                        var length = reader.ReadInt32();
                        return reader.ReadBytes(length);
                    }
                }
            }
        }


        public static object Deserialize(this IMessageSerializer serializer, Type type, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var deserialized = serializer.Deserialize(stream, new[] { type });
                if (!deserialized.Any())
                    return null;

                return deserialized[0];
            }
        }

        public static T Deserialize<T>(this IMessageSerializer serializer, byte[] bytes)
        {
            return (T) Deserialize(serializer, typeof(T), bytes);
        }
        
        public static byte[] Serialize(this IMessageSerializer serializer, object payload)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(payload, stream);
                if (stream.Length > int.MaxValue)
                    throw new ArgumentException("serialized data too long");

                stream.Position = 0;
                using (var reader = new BinaryReader(stream, Utf8NoBom))
                {
                    return reader.ReadBytes((int)stream.Length);
                }
            }
        }
        
    }
}
