using Aggregates.Contracts;
using Aggregates.DI;
using Aggregates.Exceptions;
using Aggregates.Internal;
using Aggregates.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aggregates
{
    public class Configure
    {
        // Simple entrypoints to our internal IoC if anyone needs
        public static RequestedType Resolve<RequestedType>() where RequestedType : class
        {
            return TinyIoCContainer.Current.Resolve<RequestedType>();
        }
        public static void Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            TinyIoCContainer.Current.Register<RegisterType, RegisterImplementation>();
        }
        public static void Register<RegisterType>(RegisterType instance) where RegisterType : class
        {
            TinyIoCContainer.Current.Register<RegisterType>(instance);
        }
        public static void Register<RegisterType>(Func<RegisterType> factory) where RegisterType : class
        {
            TinyIoCContainer.Current.Register<RegisterType>((container, overloads) => factory());
        }

        public Configure()
        {
            var container = TinyIoCContainer.Current;

            container.Register<IMetrics, NullMetrics>();
            container.Register<IDelayedCache, DelayedCache>();
            container.Register<IDelayedChannel, DelayedChannel>();
            container.Register<ICache, IntelligentCache>();
            container.Register<ISnapshotReader, SnapshotReader>();
            container.Register<IDomainUnitOfWork, UnitOfWork>();

            container.Register<Func<Exception, string, Error>>((factory, overloads) =>
            {
                var eventFactory = factory.Resolve<IEventFactory>();
                return (exception, message) =>
                {
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(message))
                    {
                        sb.AppendLine($"Error Message: {message}");
                    }
                    sb.AppendLine($"Exception type {exception.GetType()}");
                    sb.AppendLine($"Exception message: {exception.Message}");
                    sb.AppendLine($"Stack trace: {exception.StackTrace}");


                    if (exception.InnerException != null)
                    {
                        sb.AppendLine("---BEGIN Inner Exception--- ");
                        sb.AppendLine($"Exception type {exception.InnerException.GetType()}");
                        sb.AppendLine($"Exception message: {exception.InnerException.Message}");
                        sb.AppendLine($"Stack trace: {exception.InnerException.StackTrace}");
                        sb.AppendLine("---END Inner Exception---");

                    }
                    var aggregateException = exception as System.AggregateException;
                    if (aggregateException == null)
                        return eventFactory.Create<Error>(e => { e.Message = sb.ToString(); });

                    sb.AppendLine("---BEGIN Aggregate Exception---");
                    var aggException = aggregateException;
                    foreach (var inner in aggException.InnerExceptions)
                    {

                        sb.AppendLine("---BEGIN Inner Exception--- ");
                        sb.AppendLine($"Exception type {inner.GetType()}");
                        sb.AppendLine($"Exception message: {inner.Message}");
                        sb.AppendLine($"Stack trace: {inner.StackTrace}");
                        sb.AppendLine("---END Inner Exception---");
                    }

                    return eventFactory.Create<Error>(e =>
                    {
                        e.Message = sb.ToString();
                    });
                };
            });

            container.Register<Func<Accept>>((factory, overloads) =>
            {
                var eventFactory = factory.Resolve<IEventFactory>();
                return () => eventFactory.Create<Accept>(x => { });
            });

            container.Register<Func<string, Reject>>((factory, overloads) =>
            {
                var eventFactory = factory.Resolve<IEventFactory>();
                return message => { return eventFactory.Create<Reject>(e => { e.Message = message; }); };
            });
            container.Register<Func<BusinessException, Reject>>((factory, overloads) =>
            {
                var eventFactory = factory.Resolve<IEventFactory>();
                return exception => {
                    return eventFactory.Create<Reject>(e => {
                        e.Message = "Exception raised";
                    });
                };
            });
        }
    }
}
