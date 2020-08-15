﻿using NServiceBus;
using NServiceBus.Pipeline;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Aggregates.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class MessageExtensions
    {
        public static MessageIntentEnum GetMessageIntent(this IIncomingLogicalMessageContext message)
        {
            var messageIntent = default(MessageIntentEnum);

            if (message.MessageHeaders.TryGetValue(Headers.MessageIntent, out var messageIntentString))
            {
                Enum.TryParse(messageIntentString, true, out messageIntent);
            }

            return messageIntent;
        }
        public static MessageIntentEnum GetMessageIntent(this IOutgoingLogicalMessageContext message)
        {
            var messageIntent = default(MessageIntentEnum);

            if (message.Headers.TryGetValue(Headers.MessageIntent, out var messageIntentString))
            {
                Enum.TryParse(messageIntentString, true, out messageIntent);
            }

            return messageIntent;
        }
    }
}
