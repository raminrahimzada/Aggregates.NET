﻿using System;
using System.Threading.Tasks;

namespace Aggregates.Contracts
{
    public interface IProcessor
    {
        Task<TResponse> Process<TService, TResponse>(TService service, IContainer container) where TService : IService<TResponse>;
        Task<TResponse> Process<TService, TResponse>(Action<TService> service, IContainer container) where TService : IService<TResponse>;
    }
}
