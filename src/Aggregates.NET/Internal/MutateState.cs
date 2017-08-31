using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Aggregates.Contracts;
using Aggregates.Extensions;
using Aggregates.Messages;

namespace Aggregates.Internal
{
    // original: https://github.com/mfelicio/NDomain/blob/d30322bc64105ad2e4c961600ae24831f675b0e9/source/NDomain/StateMutator.cs

    internal static class StateMutators
    {
        static readonly ConcurrentDictionary<Type, IMutateState> Mutators;

        static StateMutators()
        {
            Mutators = new ConcurrentDictionary<Type, IMutateState>();
        }

        public static IMutateState For(Type stateType)
        {
            return Mutators.GetOrAdd(stateType, t => CreateMutator(stateType));
        }

        private static IMutateState CreateMutator(Type stateType)
        {
            var mutatorType = typeof(StateMutator<>).MakeGenericType(stateType);
            var mutator = Activator.CreateInstance(mutatorType);

            return mutator as IMutateState;
        }
    }

    internal class StateMutator<TState> : IMutateState
        where TState : IState
    {
        readonly Dictionary<string, Action<TState, object>> _mutators;

        public StateMutator()
        {
            _mutators = ReflectionExtensions.GetStateMutators<TState>();
        }

        public void Handle(IState state, IEvent @event)
        {
            // Todo: can suport "named" events with an attribute here so instead of routing based on object type 
            // route based on event name.
            var eventMutator = _mutators[$"Handle.{@event.GetType().Name}"];
            eventMutator((TState)state, @event);
        }
        public void Conflict(IState state, IEvent @event)
        {
            // Todo: can suport "named" events with an attribute here so instead of routing based on object type 
            // route based on event name.
            var eventMutator = _mutators[$"Conflict.{@event.GetType().Name}"];
            eventMutator((TState)state, @event);
        }
    }
}
