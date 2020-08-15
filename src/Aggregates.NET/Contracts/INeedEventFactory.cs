
namespace Aggregates.Contracts
{
    internal interface INeedEventFactory
    {
        IEventFactory EventFactory { get; set; }
    }
}
