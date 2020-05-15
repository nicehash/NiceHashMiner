using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.CCMinerCommon;

namespace CCMinerTpruvot
{
    public class CCMinerTpruvot : CCMinerBase
    {
        public CCMinerTpruvot(string uuid) : base(uuid)
        { }

        protected override string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);
    }
}
