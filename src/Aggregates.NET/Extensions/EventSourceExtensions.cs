using System.Linq;

namespace Aggregates.Extensions
{
    internal static class EventSourceExtensions
    {
        public static string BuildParentsString(this Id[] parents)
        {
            if (parents == null || !parents.Any())
                return "";
            return parents.Select(x => x.ToString()).Aggregate((cur, next) => $"{cur}:{next}");
        }
    }
}
