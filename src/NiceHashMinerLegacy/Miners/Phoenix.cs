using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    // Phoenix has an almost identical interface to CD, so reuse all that code
    public class Phoenix : ClaymoreDual
    {
        public Phoenix() : base(AlgorithmType.NONE)
        {
            LookForStart = "main eth speed: ";
        }
    }
}
