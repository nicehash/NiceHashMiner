
namespace NiceHashMiner.Mining.IntegratedPlugins
{
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

    class XmrStakIntegratedPlugin : XmrStak.XmrStakPlugin, IntegratedPlugin
    {
        public override string PluginUUID => "XmrStak";

        public bool Is3rdParty => false;
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

    class BrokenPluginIntegratedPlugin : BrokenMiner.BrokenMinerPlugin, IntegratedPlugin
    {
        public bool Is3rdParty => false;
    }

    class EthlargementIntegratedPlugin : Ethlargement.Ethlargement, IntegratedPlugin
    {
        public static EthlargementIntegratedPlugin Instance { get; } = new EthlargementIntegratedPlugin();
        EthlargementIntegratedPlugin() : base("Ethlargement")
        { }

        public bool Is3rdParty => true;
    }
}
