using System;
using System.Collections.Generic;
using System.Text;
using Aggregates.Contracts;
using NServiceBus.MessageInterfaces;
using System.Threading.Tasks;
using System.Threading;

namespace Aggregates.Internal
{
    public class EventMapper : IEventMapper
    {
        private readonly IMessageMapper _mapper;

        public EventMapper(IMessageMapper mapper)
        {
            _mapper = mapper;
        }

        public Type GetMappedTypeFor(Type type)
        {
            while (!Bus.BusOnline)
                Thread.Sleep(100);

            // Due to https://github.com/Particular/NServiceBus/issues/5092
            // Mapper is lazy initiated from messages received via the transport
            // because EventStore delivers events directly we need to "populate" message mapper ourselves
            _mapper.Initialize(new[] { type });
            return _mapper.GetMappedTypeFor(type);
        }
    }
}
