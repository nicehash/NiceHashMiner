using MinerPluginToolkitV1.SgminerCommon;

namespace SgminerGM
{
    public class SgminerGM : SGMinerBase
    {
        public SgminerGM(string uuid) : base(uuid)
        { }

        protected override string AlgoName => PluginSupportedAlgorithms.AlgorithmName(_algorithmType);
    }
}
