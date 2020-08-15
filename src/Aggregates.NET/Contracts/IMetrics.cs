﻿using System;

namespace Aggregates.Contracts
{
    public enum Unit
    {
        Event,
        Message,
        Command,
        Items,
        Errors
    }

    public interface IMetrics
    {
        void Mark(string name, Unit unit, long? value = null);

        void Increment(string name, Unit unit, long? value = null);
        void Decrement(string name, Unit unit, long? value = null);

        void Update(string name, Unit unit, long value);

        ITimer Begin(string name);
    }

    public interface ITimer : IDisposable {
        TimeSpan Elapsed { get; }
    }
}
