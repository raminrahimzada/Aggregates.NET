using Aggregates.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Aggregates.Internal
{
    [ExcludeFromCodeCoverage]
    internal class ServiceCollection : IContainer
    {
        private readonly IServiceCollection _serviceCollection;

        public ServiceCollection(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void Dispose()
        {
        }

        private ServiceLifetime ConvertLifestyle(Lifestyle lifestyle)
        {
            switch (lifestyle)
            {
                case Lifestyle.PerInstance:
                    return ServiceLifetime.Transient;
                case Lifestyle.Singleton:
                    return ServiceLifetime.Singleton;
                case Lifestyle.UnitOfWork:
                    return ServiceLifetime.Scoped;
            }
            throw new ArgumentException($"Unknown lifestyle {lifestyle}");
        }

        public void Register(Type concrete, Lifestyle lifestyle)
        {
            _serviceCollection.TryAdd(new ServiceDescriptor(concrete, concrete, ConvertLifestyle(lifestyle)));
            RegisterInterfaces(concrete);
        }
        public void Register<TInterface>(TInterface instance, Lifestyle lifestyle) 
        {
            _serviceCollection.TryAdd(new ServiceDescriptor(instance.GetType(), _ => instance, ConvertLifestyle(lifestyle))); 
            RegisterInterfaces(typeof(TInterface));
        }
        public void Register(Type componentType, object instance, Lifestyle lifestyle)
        {
            _serviceCollection.TryAdd(new ServiceDescriptor(componentType, _ => instance, ConvertLifestyle(lifestyle)));
            RegisterInterfaces(componentType);
        }

        public void Register<TInterface>(Func<IContainer, TInterface> factory, Lifestyle lifestyle, string name = null) 
        {
            // todo: support name? or remove name?

            _serviceCollection.TryAdd(new ServiceDescriptor(typeof(TInterface), provider => factory(new ServiceProvider(provider)), ConvertLifestyle(lifestyle)));
            RegisterInterfaces(typeof(TInterface));
        }
        public void Register<TInterface, TConcrete>(Lifestyle lifestyle, string name = null)
        {
            _serviceCollection.TryAdd(new ServiceDescriptor(typeof(TInterface), typeof(TConcrete), ConvertLifestyle(lifestyle)));
            RegisterInterfaces(typeof(TInterface));
        }
        public bool HasService(Type componentType)
        {
            return _serviceCollection.Any(sd => sd.ServiceType == componentType && sd.ImplementationType != null);
        }

        private void RegisterInterfaces(Type component)
        {
            var interfaces = component.GetInterfaces();
            foreach (var serviceType in interfaces)
            {
                if (HasService(serviceType))
                    continue;

                // see https://andrewlock.net/how-to-register-a-service-with-multiple-interfaces-for-in-asp-net-core-di/
                _serviceCollection.Add(new ServiceDescriptor(serviceType, sp => sp.GetService(component), ServiceLifetime.Transient));
            }
        }

        public object Resolve(Type resolve)
        {
            throw new NotImplementedException();
        }

        public TResolve Resolve<TResolve>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TResolve> ResolveAll<TResolve>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveAll(Type resolve)
        {
            throw new NotImplementedException();
        }

        public object TryResolve(Type resolve)
        {
            throw new NotImplementedException();
        }

        public TResolve TryResolve<TResolve>()
        {
            throw new NotImplementedException();
        }

        public IContainer GetChildContainer()
        {
            throw new NotImplementedException();
        }
    }
}
