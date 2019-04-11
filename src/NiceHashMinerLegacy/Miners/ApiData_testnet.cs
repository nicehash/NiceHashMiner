// TESTNET
#if TESTNET || TESTNETDEV

using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class ApiData
    {
        protected const double Mult = 0.000000001;

        public readonly AlgorithmType AlgorithmID;
        public readonly AlgorithmType SecondaryAlgorithmID;
        public readonly string AlgorithmName;

        public virtual double Speed { get; set; }
        public virtual double SecondarySpeed { get; set; }
        public double PowerUsage => PowerMap.Values.Sum();

        public double Revenue => (Speed * SmaVal + SecondarySpeed * SecondarySmaVal) * Mult;
        public double PowerCost => ExchangeRateApi.GetKwhPriceInBtc() * PowerUsage * 24 / 1000;
        public double Profit => Revenue - PowerCost;

        public double SmaVal;
        public double SecondarySmaVal;

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

        public double PowerCostForIndex(int index)
        {
            PowerMap.TryGetValue(index, out var pow);
            return ExchangeRateApi.GetKwhPriceInBtc() * pow * 24 / 1000;
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

        public double RevenueForIndex(int index)
        {
            Speeds.TryGetValue(index, out var sp);
            SecondarySpeeds.TryGetValue(index, out var ssp);

            return (sp * SmaVal + ssp * SecondarySmaVal) * Mult;
        }

        public double ProfitForIndex(int index)
        {
            return RevenueForIndex(index) - PowerCostForIndex(index);
        }
    }
}
#endif
