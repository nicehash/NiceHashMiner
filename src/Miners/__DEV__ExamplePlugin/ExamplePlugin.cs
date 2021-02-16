using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
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
        public Version Version => new Version(1, 2);

        /// <summary>
        /// CanGroup checks if miner can run multiple devices with same algorithm in one miner instance
        /// In following example we check if the algorithm type of first pair is DaggerHashimoto - in that case we don't group them
        /// Otherwise we check if Algorithm of first pair is same to algorithm in second pair and group them if they are same
        /// </summary>
        public bool CanGroup(MiningPair a, MiningPair b)
        {
            // we can't combine KAWPOW for some arbitrary reason on this miner
            if (a.Algorithm.FirstAlgorithmType == AlgorithmType.KAWPOW) return false;
            // other algorithms can be combined
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
                // all support KAWPOW and BeamV3
                var algorithms = new List<Algorithm> { new Algorithm(PluginUUID, AlgorithmType.KAWPOW), new Algorithm(PluginUUID, AlgorithmType.BeamV3) };

                // GPUs support DaggerHashimoto
                if (device is IGpuDevice)
                {
                    algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto));
                }
                // only NVIDIA supports Lyra2REv3
                if (device.DeviceType == DeviceType.NVIDIA)
                {
                    algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3));
                }
                // only AMD supports 
                if (device.DeviceType == DeviceType.AMD)
                {
                    algorithms.Add(new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo32));
                }

                supported.Add(device, algorithms);
            }

            return supported;
        }
    }
}
