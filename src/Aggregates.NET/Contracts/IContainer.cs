using System;
using System.Collections.Generic;
using System.Text;

namespace Aggregates.Contracts
{
    public interface IContainer
    {
        void RegisterSingleton<TInterface, TConcrete>(string name = null) where TInterface : class where TConcrete : class, TInterface;
        void RegisterSingleton<TInterface>(TInterface instance, string name = null) where TInterface : class;
        void RegisterSingleton<TInterface>(Func<IContainer, TInterface> factory, string name = null) where TInterface : class;

        void Register<TInterface, TConcrete>(string name = null) where TInterface : class where TConcrete : class, TInterface;
        void Register<TInterface>(Func<IContainer, TInterface> factory, string name = null) where TInterface : class;

        TResolve Resolve<TResolve>() where TResolve : class;
        IEnumerable<TResolve> ResolveAll<TResolve>() where TResolve : class;
    }
}
