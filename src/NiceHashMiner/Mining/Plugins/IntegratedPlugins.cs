using MinerPlugin;

// This is just a list of miners that are intergated in the nhm client
namespace NiceHashMiner.Mining.Plugins
{
    public interface IntegratedPlugin : IMinerPlugin
    {
        bool Is3rdParty { get; }
    }

    #region TESTING
    class BrokenPluginIntegratedPlugin : BrokenMiner.BrokenMinerPlugin, IntegratedPlugin
    {
        public bool Is3rdParty => false;
    }
    #endregion TESTING

    #region Open Source

    class CCMinerMTPIntegratedPlugin : CCMinerMTP.CCMinerMTPPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "CCMinerMTP";

        public bool Is3rdParty => false;
    }

    class CCMinerTpruvotIntegratedPlugin : CCMinerTpruvot.CCMinerTpruvotPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "CCMinerTpruvot";

        public bool Is3rdParty => false;
    }

    class SGminerAvemoreIntegratedPlugin : SgminerAvemore.SgminerAvemorePlugin, IntegratedPlugin
    {
        public override string PluginUUID => "SGminerAvemore";

        public bool Is3rdParty => false;
    }

    class SGminerGMIntegratedPlugin : SgminerGM.SgminerGMPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "SGminerGM";

        public bool Is3rdParty => false;
    }

    class XmrStakIntegratedPlugin : XmrStak.XmrStakPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "XmrStak";

        public bool Is3rdParty => false;
    }
    #endregion Open Source

    #region Closed source / 3rd party

    class ClaymoreDual14IntegratedPlugin : ClaymoreDual14.ClaymoreDual14Plugin, IntegratedPlugin
    {
        public override string PluginUUID => "ClaymoreDual14+";

        public bool Is3rdParty => true;
    }

    class BMinerIntegratedPlugin : BMiner.BMinerPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "BMiner";

        public bool Is3rdParty => true;
    }


    class WildRigIntegratedPlugin : WildRig.WildRigPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "WildRig";

        public bool Is3rdParty => true;
    }

    class TTMinerIntegratedPlugin : TTMiner.TTMinerPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "TTMiner";

        public bool Is3rdParty => true;
    }

    class TRexIntegratedPlugin : TRex.TRexPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "TRex";

        public bool Is3rdParty => true;
    }

    class TeamRedMinerIntegratedPlugin : TeamRedMiner.TeamRedMinerPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "TeamRedMiner";

        public bool Is3rdParty => true;
    }

    class NBMinerIntegratedPlugin : NBMiner.NBMinerPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "NBMiner";

        public bool Is3rdParty => true;
    }

    class PhoenixIntegratedPlugin : Phoenix.PhoenixPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "Phoenix";

        public bool Is3rdParty => true;
    }

    class NanoMinerIntegratedPlugin : NanoMiner.NanoMinerPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "NanoMiner";

        public bool Is3rdParty => true;
    }

    class GMinerIntegratedPlugin : GMinerPlugin.GMinerPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "GMiner";

        public bool Is3rdParty => true;
    }

    class EWBFIntegratedPlugin : EWBF.EwbfPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "Ewbf";

        public bool Is3rdParty => true;
    }

    class CryptoDredgeIntegratedPlugin : CryptoDredge.CryptoDredgePlugin, IntegratedPlugin
    {
        public override string PluginUUID => "CryptoDredge";

        public bool Is3rdParty => true;
    }

    class EthlargementIntegratedPlugin : Ethlargement.Ethlargement, IntegratedPlugin
    {
        public static EthlargementIntegratedPlugin Instance { get; } = new EthlargementIntegratedPlugin();

        public override string PluginUUID => "Ethlargement";

        public bool Is3rdParty => true;
    }

    #endregion Closed source / 3rd party
}
