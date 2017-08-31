using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Aggregates.DI;
using Aggregates.Messages;

namespace Aggregates.Internal
{
    class Processor : IProcessor
    {
        [DebuggerStepThrough]
        public Task<IEnumerable<TResponse>> Process<TQuery, TResponse>(TQuery query) where TQuery : IQuery<TResponse>
        {
            var handlerType = typeof(IHandleQueries<,>).MakeGenericType(typeof(TQuery), typeof(TResponse));
            
            dynamic handler = TinyIoCContainer.Current.Resolve(handlerType);

            return handler.Handle((dynamic)query);
        }
    }
}
