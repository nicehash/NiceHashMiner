using System.Threading;
using System.Threading.Tasks;

namespace MinerPlugin
{
    public interface IMinerAsyncExtensions : IMiner
    {
        Task MinerProcessTask { get; }
        Task<object> StartMiningTask(CancellationToken stop);
        Task StopMiningTask();
    }
}
