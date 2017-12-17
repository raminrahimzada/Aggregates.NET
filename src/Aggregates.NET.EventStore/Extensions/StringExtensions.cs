

namespace Aggregates.Extensions
{
    static class StringExtensions
    {
        // dotnet core GetHashCode returns different values (not deterministic)
        // we just need a simple deterministic hash
        public static int GetHash(this string stream)
        {
            // Unchecked, allow overflow
            unchecked
            {
                int hash = 23;
                foreach (char c in stream)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }
    }
}
