
namespace NHM.MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IAfterStartMining interface is used by MinerBase <see cref="MinerBase"/> to execute after starting the mining process.
    /// If you are deriving from MinerBase and need to execute an action after starting the mining process, implement this interface.
    /// </summary>
    public interface IAfterStartMining
    {
        void AfterStartMining();
    }
}
