namespace Aggregates
{
    public enum ConcurrencyConflict
    {
        Throw,
        Ignore,
        Discard,
        ResolveStrongly,
        ResolveWeakly,
        Custom
    }
}
