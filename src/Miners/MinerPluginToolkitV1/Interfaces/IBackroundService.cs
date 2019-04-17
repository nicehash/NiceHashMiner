using MinerPlugin;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.Interfaces
{
    public interface IBackroundService
    {
        bool ServiceEnabled { get; set; }
        void Start(IEnumerable<MiningPair> miningPairs);
        void Stop(IEnumerable<MiningPair> miningPairs = null);
    }
}
