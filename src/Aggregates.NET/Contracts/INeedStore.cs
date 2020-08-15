namespace Aggregates.Contracts
{
    internal interface INeedStore
    {
        IStoreEvents Store { get; set; }
        IOobWriter OobWriter { get; set; }
    }
}
