using MinerPlugin;

namespace NiceHashMiner.Mining.IntegratedPlugins
{
    public interface IntegratedPlugin : IMinerPlugin
    {
        bool Is3rdParty { get; }
    }
}
