using MinerPlugin;
using System.Collections.Generic;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public interface IntegratedPlugin : IMinerPlugin
    {
        bool Is3rdParty { get; }

        // IMinerBinsSource
        /// <summary>
        /// Return ordered urls where we can download miner binary files
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetMinerBinsUrls();
    }
}
