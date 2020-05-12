using NHM.Common.Algorithm;
using NHM.Common.Device;
using System;
using System.Collections.Generic;

namespace NHM.MinerPlugin
{
    /// <summary>
    /// IMinerPlugin is the base interface for registering a plugin in NiceHashMiner.
    /// This Interface should convey the name, version, grouping logic and most importantly should filter supported devices and algorithms.
    /// </summary>
    public interface IMinerPlugin
    {
        /// <summary>
        /// Specifies the plugin version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Specifies the plugin name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Specifies the plugin author.
        /// </summary>
        string Author { get; }


        /// <summary>
        /// Checks supported devices for the plugin and returns devices and algorithms that can be mined with the plugin.
        /// </summary>
        /// <param name="devices"></param>
        Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices);

        /// <summary>
        /// Creates the plugin miner instance that is used for mining inside NiceHashMiner. 
        /// </summary>
        /// <returns>Returns the underlying IMiner instance.</returns>
        IMiner CreateMiner();

        /// <summary>
        /// UUID for the plugin.
        /// </summary>
        string PluginUUID { get; }

        /// <summary>
        /// Checks if mining pairs a and b can be executed inside the same miner (IMiner) instance.
        /// For example if we want to mine NeoScrypt on the two GPUs with ccminer we will create only one miner instance and run both on it.
        /// On certain miners like cpuminer if we would have dual socket CPUs and would mine the same algorithm we would run two instances.
        /// This is case by case and it depends on the miner.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        bool CanGroup(MiningPair a, MiningPair b);
    }
}
