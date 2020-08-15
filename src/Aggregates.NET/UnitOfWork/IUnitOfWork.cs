using System;
using System.Threading.Tasks;

namespace Aggregates.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task Begin();
        Task End(Exception ex = null);
    }
}
