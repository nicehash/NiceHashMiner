using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class ApiData
    {
        public readonly AlgorithmType AlgorithmID;
        public readonly AlgorithmType SecondaryAlgorithmID;
        public readonly string AlgorithmName;
        public double Speed;
        public double SecondarySpeed;
        public double PowerUsage;

        public readonly List<ComputeDevice> Devices;

        public IEnumerable<int> DeviceIndices => Devices.Select(d => d.Index);

        public ApiData(AlgorithmType algo, IEnumerable<ComputeDevice> devices, AlgorithmType secondaryAlgo = AlgorithmType.NONE)
        {
            AlgorithmID = algo;
            SecondaryAlgorithmID = secondaryAlgo;
            AlgorithmName = AlgorithmNiceHashNames.GetName(Helpers.DualAlgoFromAlgos(algo, secondaryAlgo));
            Devices = (devices as List<ComputeDevice>) ?? devices.ToList();
        }
    }
}
