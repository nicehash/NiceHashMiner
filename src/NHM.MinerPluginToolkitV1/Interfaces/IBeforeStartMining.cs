
namespace NHM.MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IBeforeStartMining interface is used by MinerBase <see cref="MinerBase"/> to execute before starting the mining process.
    /// If you are deriving from MinerBase and need to execute an action before starting the mining process, implement this interface.
    /// </summary>
    public interface IBeforeStartMining
    {
        void BeforeStartMining();
    }
}
