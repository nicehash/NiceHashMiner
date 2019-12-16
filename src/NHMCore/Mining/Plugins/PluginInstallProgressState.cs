
namespace NHMCore.Mining.Plugins
{
    public enum PluginInstallProgressState
    {
        Pending,
        // plugin dll
        PendingDownloadingPlugin,
        DownloadingPlugin,
        PendingExtractingPlugin,
        ExtractingPlugin,
        // miner bin
        PendingDownloadingMiner,
        DownloadingMiner,
        PendingExtractingMiner,
        ExtractingMiner,
        // failed cases
        FailedDownloadingPlugin,
        FailedExtractingPlugin,
        FailedDownloadingMiner,
        FailedExtractingMiner,
        // installing
        FailedPluginLoad,
        FailedPluginInit,
        FailedUnknown,
        Success,
        Canceled
    }
}
