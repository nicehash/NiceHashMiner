using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1.CCMinerCommon;
using NHM.Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CCMinerTpruvot
{
    public class CCMinerTpruvot : CCMinerBase
    {
        public CCMinerTpruvot(string uuid) : base(uuid)
        { }

        protected override string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);
    }
}
