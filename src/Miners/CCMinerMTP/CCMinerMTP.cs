using MinerPluginToolkitV1.CCMinerCommon;
using NHM.Common.Enums;

namespace CCMinerMTP
{
    public class CCMinerMTP : CCMinerBase
    {
        public CCMinerMTP(string uuid) : base(uuid)
        { }

        protected override string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);
    }
}
