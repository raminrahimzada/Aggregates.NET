using System;

namespace Aggregates.Exceptions
{
    public class SerializerException : Exception
    {
        public SerializerException(Exception inner, string path) : base($"Serialization exception on '{path}'", inner) { }
    }
}
