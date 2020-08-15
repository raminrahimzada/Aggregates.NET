using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Diagnostics.CodeAnalysis;

namespace Aggregates
{
    [ExcludeFromCodeCoverage]
    public static class SIConfigure
    {
        public static Configure SimpleInjector(this Configure config, Container container)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.AllowOverridingRegistrations = true;

            config.Container = new Internal.Container(container);

            return config;
        }
    }
}
