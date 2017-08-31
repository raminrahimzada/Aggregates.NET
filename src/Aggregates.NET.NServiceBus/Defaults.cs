using System;
using System.Collections.Generic;
using System.Text;

namespace Aggregates.NServiceBus
{
    public static class Defaults
    {
        // Header information to take from incoming messages
        public static IList<string> CarryOverHeaders = new List<string>
        {
            "NServiceBus.MessageId",
            "NServiceBus.CorrelationId",
            "NServiceBus.Version",
            "NServiceBus.TimeSent",
            "NServiceBus.ConversationId",
            "NServiceBus.OriginatingMachine",
            "NServiceBus.OriginatingEndpoint"
        };
        public static string MessageIdHeader = "NServiceBus.MessageId";
    }
}
