using System;
using Aggregates.Contracts;
using Aggregates.Messages;

namespace Aggregates.Internal
{
    internal class FullEvent : IFullEvent
    {
        public IEventDescriptor Descriptor { get; set; }
        public IEvent Event { get; set; }
        public Guid? EventId { get; set; }
    }
}
