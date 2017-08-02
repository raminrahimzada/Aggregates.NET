using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aggregates.Contracts;

namespace Aggregates
{
    public class State : IState, INeedStream
    {
        public Id Id { get; internal set; }
        public long Version { get; internal set; }

        internal IEventStream Stream => (this as INeedStream).Stream;
        IEventStream INeedStream.Stream { get; set; }
        IEventStream IEventSource.Stream => (this as INeedStream).Stream;

        IEventSource IEventSource.Parent => null;
    }

    public class State<TParent> : IState, INeedStream where TParent : IEventSource
    {
        public Id Id { get; internal set; }
        public long Version { get; internal set; }

        internal IEventStream Stream => (this as INeedStream).Stream;
        IEventStream INeedStream.Stream { get; set; }
        IEventStream IEventSource.Stream => (this as INeedStream).Stream;

        IEventSource IEventSource.Parent => Parent;

        public TParent Parent { get; internal set; }
    }
}
