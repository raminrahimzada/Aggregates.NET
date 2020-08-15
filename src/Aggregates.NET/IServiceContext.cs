using Aggregates.Contracts;

namespace Aggregates
{
    public interface IServiceContext
    {
        UnitOfWork.IDomain Domain { get; }
        UnitOfWork.IApplication App { get; }
        IProcessor Processor { get; }

        IContainer Container { get; }
    }
}
