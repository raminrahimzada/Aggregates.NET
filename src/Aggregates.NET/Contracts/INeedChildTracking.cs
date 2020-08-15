namespace Aggregates.Contracts
{
    internal interface INeedChildTracking
    {
        ITrackChildren Tracker { get; set; }
    }
}
