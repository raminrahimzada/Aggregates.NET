using System;
using System.Diagnostics.CodeAnalysis;

namespace Aggregates.Internal
{
    [ExcludeFromCodeCoverage]
    internal class TestableVersionRegistrar : Contracts.IVersionRegistrar
    {
        public Type GetNamedType(string versionedName)
        {
            throw new NotImplementedException();
        }

        public string GetVersionedName(Type versionedType)
        {
            return $"Testing.{versionedType.FullName}";
        }

        public void Load(Type[] types)
        {
            throw new NotImplementedException();
        }
    }
}
