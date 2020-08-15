﻿using Aggregates.Contracts;
using Newtonsoft.Json;
using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Aggregates.Internal
{
    [ExcludeFromCodeCoverage]
    internal class TestableProcessor : ITestableProcessor
    {
        private readonly TestableEventFactory _factory;

        public TestableProcessor()
        {
            _factory = new TestableEventFactory(new MessageMapper());
            Planned = new Dictionary<string, object>();
            Requested = new List<string>();
        }

        internal Dictionary<string, object> Planned;
        internal List<string> Requested;

        public IServicePlanner<TService, TResponse> Plan<TService, TResponse>(TService service) where TService : IService<TResponse>
        {
            return new ServicePlanner<TService, TResponse>(this, service);
        }
        public IServiceChecker<TService, TResponse> Check<TService, TResponse>(TService service) where TService : IService<TResponse>
        {
            return new ServiceChecker<TService, TResponse>(this, service);
        }
        public Task<TResponse> Process<TService, TResponse>(TService service, IContainer container) where TService : IService<TResponse>
        {
            var serviceString = JsonConvert.SerializeObject(service);
            if (!Planned.ContainsKey($"{typeof(TService).FullName}.{serviceString}"))
                throw new ArgumentException($"Service {typeof(TService).FullName} body {serviceString} was not planned");
            return Task.FromResult((TResponse)Planned[$"{typeof(TService).FullName}.{serviceString}"]);
        }

        public Task<TResponse> Process<TService, TResponse>(Action<TService> service, IContainer container) where TService : IService<TResponse>
        {
            return Process<TService, TResponse>(_factory.Create(service), container);
        }
    }
}
