using Aggregates.Logging;
using NServiceBus;
using NServiceBus.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.Internal
{
    class LogContextProviderBehaviour : Behavior<IIncomingPhysicalMessageContext>
    {

        public override Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
        {
            // Populate the logging context with useful data from the messaeg
            using (LogProvider.OpenMappedContext("Instance", Defaults.Instance.ToString()))
            {
                using (LogProvider.OpenMappedContext("MessageId", context.MessageId))
                {
                    string corrId = "";
                    context.MessageHeaders.TryGetValue(Headers.CorrelationId, out corrId);
                    if (string.IsNullOrEmpty(corrId))
                        corrId = context.MessageId;
                    using (LogProvider.OpenMappedContext("CorrId", corrId))
                    {
                        using (LogProvider.OpenMappedContext("Endpoint", Configuration.Settings.Endpoint))
                        {
                            return next();
                        }
                    }
                }
            }
        }
    }
}
