namespace Aggregates.Contracts
{
    interface INeedRepositoryFactory
    {
        IRepositoryFactory RepositoryFactory { get; set; }
    }
}
