using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Aggregates.Contracts;

namespace Aggregates.Internal
{
    [ExcludeFromCodeCoverage]
    internal class Mutating : IMutating
    {
        public Mutating(object message, IDictionary<string, string> headers)
        {
            Message = message;
            Headers = new Dictionary<string, string>(headers.ToDictionary(x => x.Key, x => x.Value));
        }

        public object Message { get; set; }
        public IDictionary<string, string> Headers { get; }
    }
}
