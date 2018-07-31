using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class ApiData
    {
        public readonly AlgorithmType AlgorithmID;
        public readonly AlgorithmType SecondaryAlgorithmID;
        public readonly string AlgorithmName;

        public virtual double Speed { get; set; }
        public virtual double SecondarySpeed { get; set; }
        public double PowerUsage => PowerMap.Values.Sum();

        public double Profit;
        public double Revenue;

        public readonly List<int> DeviceIndices;
        public readonly Dictionary<int, double> PowerMap = new Dictionary<int, double>();

        public ApiData(AlgorithmType algorithmID, List<int> indices, AlgorithmType secondaryAlgorithmID = AlgorithmType.NONE)
        {
            AlgorithmID = algorithmID;
            SecondaryAlgorithmID = secondaryAlgorithmID;
            AlgorithmName = AlgorithmNiceHashNames.GetName(Helpers.DualAlgoFromAlgos(algorithmID, secondaryAlgorithmID));
            DeviceIndices = indices;
        }

        public ApiData(MiningSetup setup)
            : this(setup.CurrentAlgorithmType, setup.MiningPairs.Select(p => p.Device.Index).ToList(), setup.CurrentSecondaryAlgorithmType)
        {
            foreach (var pair in setup.MiningPairs)
            {
                var measuredUsage = pair.Device.PowerUsage;
                if (measuredUsage >= 0)
                {
                    PowerMap[pair.Device.Index] = measuredUsage;
                }
                else
                {
                    // Fall back on user set vals
                    PowerMap[pair.Device.Index] = pair.Algorithm.PowerUsage;
                }
            }
        }
    }

    public class SplitApiData : ApiData
    {
        public readonly Dictionary<int, double> Speeds = new Dictionary<int, double>();
        public readonly Dictionary<int, double> SecondarySpeeds = new Dictionary<int, double>();

        public override double Speed => Speeds.Values.Sum();
        public override double SecondarySpeed => SecondarySpeeds.Values.Sum();

        public SplitApiData(MiningSetup setup)
            : base(setup)
        { }
    }
}
