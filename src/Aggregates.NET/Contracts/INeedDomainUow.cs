namespace Aggregates.Contracts
{
    internal interface INeedDomainUow
    {
        UnitOfWork.IDomain Uow { get; set; }
    }
}
