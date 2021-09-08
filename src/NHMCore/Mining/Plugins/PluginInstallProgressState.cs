
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
        FailedWrongHashPlugin,
        FailedDownloadingMiner,
        FailedExtractingMiner,
        FailedWrongHashMiner,
        // installing
        FailedPluginLoad,
        FailedPluginInit,
        FailedUnknown,
        Success,
        Canceled
    }
}
