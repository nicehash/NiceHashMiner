using MinerPlugin;
using NiceHashMinerLegacy.Common.Device;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class XmrStakIntegratedPlugin : XmrStak.XmrStakPlugin, IntegratedPlugin
    {
        public XmrStakIntegratedPlugin() : base("XmrStak")
        { }

        public bool Is3rdParty => false;

        public new IMiner CreateMiner()
        {
            return new XmrStakIntegratedMiner(PluginUUID, AMDDevice.OpenCLPlatformID, this)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }
    }
}
