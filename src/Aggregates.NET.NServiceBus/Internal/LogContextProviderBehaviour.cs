using Aggregates.Extensions;
using Aggregates.Logging;
using NServiceBus;
using NServiceBus.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.Internal
{
    class LogContextProviderBehaviour : Behavior<IIncomingLogicalMessageContext>
    {
        private static readonly ILog Logger = LogProvider.GetLogger("LogContextProvider");

        public override Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            // Populate the logging context with useful data from the messaeg
            using (LogProvider.OpenMappedContext("Instance", Defaults.Instance.ToString()))
            {
                string messageId = "";
                context.MessageHeaders.TryGetValue(Headers.MessageId, out messageId);

                using (LogProvider.OpenMappedContext("MessageId", messageId))
                {
                    string corrId = "";
                    context.MessageHeaders.TryGetValue(Headers.CorrelationId, out corrId);
                    if (string.IsNullOrEmpty(corrId))
                        corrId = messageId;
                    using (LogProvider.OpenMappedContext("CorrId", corrId))
                    {
                        using (LogProvider.OpenMappedContext("Endpoint", Configuration.Settings.Endpoint))
                        {
                            Logger.DebugEvent("Start", "Processing [{MessageId:l}] Corr: [{CorrelationId:l}]", messageId, corrId);
                            return next();
                        }
                    }
                }
            }
        }
    }
    internal class LogContextProviderRegistration : RegisterStep
    {
        public LogContextProviderRegistration() : base(
            stepId: "LogContextProvider",
            behavior: typeof(LogContextProviderBehaviour),
            description: "Provides useful message information to logger")
        {
            InsertAfter("LocalMessageUnpack");
        }
    }
}
