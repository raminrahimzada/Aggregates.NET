using System.Collections.Generic;

namespace Aggregates
{
    public static class Dynamic
    {
        public static bool ContainsProperty(dynamic @object, string property)
        {
            return ((IDictionary<string, object>) @object).ContainsKey(property);
        }
    }
}
