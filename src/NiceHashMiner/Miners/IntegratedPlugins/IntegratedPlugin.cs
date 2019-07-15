using MinerPlugin;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public interface IntegratedPlugin : IMinerPlugin
    {
        bool Is3rdParty { get; }
    }
}
