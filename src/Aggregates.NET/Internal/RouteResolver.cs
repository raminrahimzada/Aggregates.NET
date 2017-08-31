using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Aggregates.Contracts;
using Aggregates.Logging;

namespace Aggregates.Internal
{
    class RouteResolver
    {
        private static readonly ILog Logger = LogProvider.GetLogger("RouteResolver");

        // Yuck!
        private static readonly Dictionary<Type, IDictionary<string, Action<IState, object>>> Cache = new Dictionary<Type, IDictionary<string, Action<IState, object>>>();
        private static readonly object Lock = new object();
        private readonly IEventMapper _mapper;

        public RouteResolver(IEventMapper mapper)
        {
            _mapper = mapper;
        }

        private IDictionary<string, Action<IState, object>> GetCached(IState eventsource, Type eventType)
        {
            // Wtf is going on here? Well allow me to explain
            // In our eventsources we have methods that look like:
            // private void Handle(Events.MyEvent e) {}
            // and we may also have
            // private void Conflict(Events.MyEvent e) {}
            // this little cache GetOrAdd is basically searching for those methods and returning an Action the caller 
            // can use to execute the method 
            var mappedType = _mapper.GetMappedTypeFor(eventType) ?? eventType;
            IDictionary<string, Action<IState, object>> handles;
            lock (Lock)
            {
                if (Cache.TryGetValue(eventType, out handles))
                    return handles;
            }

            var methods = eventsource.GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(
                    m => (m.Name == "Handle" || m.Name == "Conflict") &&
                         m.GetParameters().Length == 1 &&
                         m.GetParameters().Single().ParameterType == mappedType &&
                         m.ReturnParameter.ParameterType == typeof(void));
            //.Select(m => new { Method = m, MessageType = m.GetParameters().Single().ParameterType });

            var methodInfos = methods as MethodInfo[] ?? methods.ToArray();
            if (!methodInfos.Any())
                return null;

            handles = methodInfos.ToDictionary(x => x.Name, x => (Action<IState, object>)((es, m) =>
            {
                try
                {
                    x.Invoke(es, new[] { m });
                }
                catch (TargetInvocationException e)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
            }));

            lock (Lock)
            {
                return Cache[eventType] = handles;
            }
        }

        public Action<IState, object> Resolve(IState eventsource, Type eventType)
        {
            var result = GetCached(eventsource, eventType);
            if (result == null) return null;

            Action<IState, object> handle;
            if (!result.TryGetValue("Handle", out handle))
                handle = null;
            return handle;
        }
        public Action<IState, object> Conflict(IState eventsource, Type eventType)
        {
            var result = GetCached(eventsource, eventType);
            if (result == null) return null;

            Action<IState, object> handle;
            if (!result.TryGetValue("Conflict", out handle))
                handle = null;
            return handle;
        }

    }
}
