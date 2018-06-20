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
        public double Profit;
        public double Revenue;

        public readonly List<int> DeviceIndices;

        public ApiData(AlgorithmType algorithmID, List<int> indices, AlgorithmType secondaryAlgorithmID = AlgorithmType.NONE)
        {
            AlgorithmID = algorithmID;
            SecondaryAlgorithmID = secondaryAlgorithmID;
            AlgorithmName = AlgorithmNiceHashNames.GetName(Helpers.DualAlgoFromAlgos(algorithmID, secondaryAlgorithmID));
            DeviceIndices = indices;
        }

        public ApiData(AlgorithmType algo, IEnumerable<ComputeDevice> devices,
            AlgorithmType secondaryAlgo = AlgorithmType.NONE, double totalPower = -1)
            : this(algo, devices.Select(d => d.Index).ToList(), secondaryAlgo)
        {
            if (totalPower < 0)
            {
                PowerUsage = devices.Sum(d => d.PowerUsage);
            }
        }
    }
}
