﻿using Aggregates.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Aggregates.Internal
{
    [ExcludeFromCodeCoverage]
    internal class NullMetrics : IMetrics
    {
        private class Timer : ITimer
        {
            public TimeSpan Elapsed => TimeSpan.MinValue;

            public void Dispose() { }
        }

        public ITimer Begin(string name)
        {
            return new Timer();
        }

        public void Decrement(string name, Unit unit, long? value = null)
        {
        }

        public void Increment(string name, Unit unit, long? value = null)
        {
        }

        public void Mark(string name, Unit unit, long? value = null)
        {
        }

        public void Update(string name, Unit unit, long value)
        {
        }
    }
}
