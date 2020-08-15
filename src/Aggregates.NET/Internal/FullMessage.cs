using System.Collections.Generic;
using Aggregates.Contracts;
using Aggregates.Messages;

namespace Aggregates.Internal
{
    internal class FullMessage : IFullMessage
    {
        public IMessage Message { get; set; }
        public IDictionary<string,string> Headers { get; set; }
    }
}
