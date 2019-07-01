using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;

namespace Example
{
    /// <summary>
    /// Plugin class inherits IMinerPlugin interface for registering plugin
    /// </summary>
    public class ExamplePlugin : IMinerPlugin
    {
        public string Name => "ExamplePlugin";

        /// <summary>
        /// Developer sets his email/name/nickname as Author so it can be used for contact information in case of any issues.
        /// </summary>
        public string Author => "developer@email.com";

        /// <summary>
        /// PluginUUID is an unique user identifier which you can generate online (you can use https://www.uuidgenerator.net/ for example)
        /// <summary>
        public string PluginUUID => "455c4d98-a45d-45d6-98ca-499ce866b2c7";

        /// <summary>
        /// With version we set the version of the plugin for further updating capabilities.
        /// </summary>
        public Version Version => new Version(1, 0);

        /// <summary>
        /// CanGroup checks if miner can run multiple devices with same algorithm in one miner instance
        /// In following example we check if the algorithm type of first pair is DaggerHashimoto - in that case we don't group them
        /// Otherwise we check if Algorithm of first pair is same to algorithm in second pair and group them if they are same
        /// </summary>
        public bool CanGroup(MiningPair a, MiningPair b)
        {
            if (a.Algorithm.FirstAlgorithmType == AlgorithmType.DaggerHashimoto) return false;
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        /// <summary>
        /// Creates a new miner instance
        /// </summary>
        public IMiner CreateMiner()
        {
            return new ExampleMiner();
        }

        /// <summary>
        /// GetSupportedAlgorithms returns algorithms supported by Miner.
        /// You can also apply hardware filters here.
        /// </summary>
        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // we loop through devices and add supported algorithms for that device to dictionary
            foreach (var device in devices)
            {
                if (device.DeviceType == DeviceType.CPU)
                {
                    supported.Add(device, new List<Algorithm> { new Algorithm(PluginUUID, AlgorithmType.CryptoNightR) });
                }
                else if(device.DeviceType == DeviceType.AMD)
                {
                    supported.Add(device, new List<Algorithm> { new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto) });
                }
                else // NVIDIA
                {
                    supported.Add(device, new List<Algorithm> {
                        new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto),
                        new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29)
                    });
                }
            }

            return supported;
        }
    }
}
