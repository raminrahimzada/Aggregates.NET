
using System.Threading.Tasks;

namespace Aggregates.Contracts
{
    public interface ISnapshotReader
    {
        Task<ISnapshot> Retrieve(string stream);
    }
}
