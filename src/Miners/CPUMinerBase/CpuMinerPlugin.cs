using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CPUMinerBase
{
    public class CPUMinerPlugin : IMinerPlugin
    {
        public string PluginUUID => "0d4422e2-342b-4daa-9cca-8ceb96ce4279";

        public Version Version => new Version(1,0);

        public string Name => "cpuminer";

        public string Author => "stanko@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cpus = devices.Where(dev => dev is CPUDevice).Select(dev => (CPUDevice)dev);
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var cpu in cpus)
            {
                supported.Add(cpu, GetSupportedAlgorithms());
            }

            return supported;
        }

        public IMiner CreateMiner() => new CpuMiner(PluginUUID);

        
        // TODO check get what kind of benchmark it is, local or network

        // TODO reserved miner API port
        // TODO miner connection type add ssl disable ssl, has SSL, when to use SSL does it make sense to use it? 
        // TODO add is online or offline benchmark
        // TODO DevFee does it have one what kind of a fee is there, does it differ from algo or ssl enabled
        // extra launch parameters thingy should be taken care of per miner

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b) 
        {
            return false;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms()
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.Lyra2Z)
            };
        }
    }
}
