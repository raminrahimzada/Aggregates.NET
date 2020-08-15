namespace Aggregates.Contracts
{
    internal interface INeedVersionRegistrar
    {
        IVersionRegistrar Registrar { get; set; }
    }
}
