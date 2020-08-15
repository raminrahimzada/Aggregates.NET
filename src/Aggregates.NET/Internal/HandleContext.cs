using System.Diagnostics.CodeAnalysis;
using Aggregates.Contracts;

namespace Aggregates.Internal
{
    [ExcludeFromCodeCoverage]
    internal class HandleContext : IServiceContext
    {
        public HandleContext(Aggregates.UnitOfWork.IDomain domain, Aggregates.UnitOfWork.IApplication app, IProcessor processor, IContainer container)
        {
            Domain = domain;
            App = app;
            Processor = processor;
            Container = container;
        }

        public Aggregates.UnitOfWork.IDomain Domain { get; }
        public Aggregates.UnitOfWork.IApplication App { get; }
        public IProcessor Processor { get; }
        public IContainer Container { get; }

    }
}
