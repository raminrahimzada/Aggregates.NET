using Aggregates.DI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aggregates
{
    public static class Container
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
    }
}
