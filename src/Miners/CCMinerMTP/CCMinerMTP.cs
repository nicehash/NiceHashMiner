using NHM.Common.Enums;
using MinerPluginToolkitV1.CCMinerCommon;

namespace CCMinerMTP
{
    public class CCMinerMTP : CCMinerBase
    {
        public CCMinerMTP(string uuid) : base(uuid)
        { }

        protected override string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.MTP: return "mtp";
            }
            // TODO throw exception
            return "";
        }
    }
}
