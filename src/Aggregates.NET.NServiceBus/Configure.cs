using Aggregates.DI;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregates
{
    public static class Configure
    {
        public static EndpointConfiguration WireUp(this EndpointConfiguration config, int retries)
        {
            var settings = config.GetSettings();

            var types = settings.GetAvailableTypes();

            // Register all query handlers in my IOC so query processor can use them
            foreach (var type in types.Where(IsQueryHandler))
                TinyIoCContainer.Current.Register(type);


            settings.Set("Retries", retries);

            // Set immediate retries to our "MaxRetries" setting
            config.Recoverability().Immediate(x =>
            {
                x.NumberOfRetries(retries);
            });

            config.Recoverability().Delayed(x =>
            {
                // Delayed retries don't work well with the InMemory context bag storage.  Creating
                // a problem of possible duplicate commits
                x.NumberOfRetries(0);
                //x.TimeIncrease(TimeSpan.FromSeconds(1));
                //x.NumberOfRetries(forever ? int.MaxValue : delayedRetries);
            });

            config.EnableFeature<Feature>();

            return config;
        }

        private static bool IsQueryHandler(Type type)
        {
            if (type.IsAbstract || type.IsGenericTypeDefinition)
                return false;

            return type.GetInterfaces()
                .Where(@interface => @interface.IsGenericType)
                .Select(@interface => @interface.GetGenericTypeDefinition())
                .Any(genericTypeDef => genericTypeDef == typeof(IHandleQueries<,>));
        }
    }
}
