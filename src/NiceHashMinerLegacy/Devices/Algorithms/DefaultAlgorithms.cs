using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using System.Collections.Generic;

namespace NiceHashMiner.Devices.Algorithms
{
    // TODO remove when we implement ClaymoreDual plugin
    public static class DefaultAlgorithms
    {
        // NVIDIA and AMD
        private static List<Algorithm> ClaymoreDualAlgorithmsForDevice(ComputeDevice dev)
        {
            if (dev.DeviceType == DeviceType.CPU) return null;
            // filter RAM requirements
            const ulong MinDaggerHashimotoMemory = 3UL << 30; // 3GB
            if (dev.GpuRam < MinDaggerHashimotoMemory) return null;
            // SM5.0+
            if (dev is CudaComputeDevice cudaDev && cudaDev.SMMajor < 5) return null;

            var algos = new List<Algorithm>
            {
                new Algorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto),
                // duals disabled by default
                #pragma warning disable 0618
                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred) { Enabled = false },
                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s) { Enabled = false },
                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak) { Enabled = false }
                #pragma warning restore 0618
            };
            return algos;
        }

        private delegate List<Algorithm> AlgorithmsForDevice(ComputeDevice dev);

        private static IReadOnlyList<AlgorithmsForDevice> algorithmsDelegates3rdParty = new List<AlgorithmsForDevice>
        {
            ClaymoreDualAlgorithmsForDevice,
        };

        public static List<Algorithm> GetAlgorithmsForDevice(ComputeDevice dev)
        {
            var ret = new List<Algorithm>();
            var delegates = new List<AlgorithmsForDevice>();
            // TODO add 3rdparty checking
            delegates.AddRange(algorithmsDelegates3rdParty);

            foreach (var algorithmsFor in delegates)
            {
                var algorithms = algorithmsFor(dev);
                if (algorithms != null) ret.AddRange(algorithms);
            }
            return ret;
        }
    }
}
