using MinerPluginToolkitV1.SgminerCommon;
using NHM.Common.Enums;

namespace SgminerGM
{
    public class SgminerGM : SGMinerBase
    {
        public SgminerGM(string uuid) : base(uuid)
        { }

        protected override string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.DaggerHashimoto:
                        return "ethash";
                    default:
                        return "";
                }
            }
        }
    }
}
