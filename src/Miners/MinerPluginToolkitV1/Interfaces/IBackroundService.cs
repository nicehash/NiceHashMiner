using MinerPlugin;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IBackroundService interface is used by plugins that are being used as Background service.
    /// </summary>
    /// For example check EthlargementPlugin
    public interface IBackroundService
    {
        bool ServiceEnabled { get; set; }
        void Start(IEnumerable<MiningPair> miningPairs);
        void Stop(IEnumerable<MiningPair> miningPairs = null);
    }
}
