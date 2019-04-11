// PRODUCTION
#if !(TESTNET || TESTNETDEV)

using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class ApiData
    {
        public AlgorithmType AlgorithmID;
        public AlgorithmType SecondaryAlgorithmID;
        public string AlgorithmName;
        public double Speed;
        public double SecondarySpeed;
        public double PowerUsage;

        public ApiData(AlgorithmType algorithmID, AlgorithmType secondaryAlgorithmID = AlgorithmType.NONE)
        {
            AlgorithmID = algorithmID;
            SecondaryAlgorithmID = secondaryAlgorithmID;
            AlgorithmName = AlgorithmNiceHashNames.GetName(Helpers.DualAlgoFromAlgos(algorithmID, secondaryAlgorithmID));
            Speed = 0.0;
            SecondarySpeed = 0.0;
            PowerUsage = 0.0;
        }
    }
}
#endif
