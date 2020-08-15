using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aggregates
{
    [ExcludeFromCodeCoverage]
    public static class NSBDefaults
    {
        // Header information to take from incoming messages
        public static readonly IReadOnlyCollection<string> CarryOverHeaders = new[]
        {
            "NServiceBus.MessageId",
            "NServiceBus.CorrelationId",
            "NServiceBus.Version",
            "NServiceBus.TimeSent",
            "NServiceBus.ConversationId",
            "NServiceBus.OriginatingMachine",
            "NServiceBus.OriginatingEndpoint"
        };
        public const string MessageIdHeader = "NServiceBus.MessageId";
        public const string CorrelationIdHeader = "NServiceBus.CorrelationId";
    }
}
