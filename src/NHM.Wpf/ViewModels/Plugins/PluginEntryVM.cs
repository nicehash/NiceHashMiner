using NHM.Wpf.ViewModels.Models;
using NHMCore.Mining.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using NHMCore;
using static NHMCore.Translations;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginEntryVM : BaseVM
    {
        public PluginPackageInfoCR Plugin { get; }
        private IProgress<Tuple<PluginInstallProgressState, int>> Progress;

        public string InstallString
        {
            get
            {
                if (Plugin.Installed) return Translations.Tr("Remove");
                if (Load.IsInstalling) return Translations.Tr("Installing");
                if (Plugin.Supported) return Translations.Tr("Install");
                return Translations.Tr("Not Supported");
            }
        }

        public bool InstallButtonEnabled => (Plugin.Installed || Plugin.Supported) && !Load.IsInstalling;

        // Load is shared between the listing and detail page. This allows navigating to details and back
        // while maintaining the progress.
        public LoadProgress Load { get; }

        protected readonly Dictionary<string, List<string>> FilteredSupportedAlgorithms;

        public PluginEntryVM(PluginPackageInfoCR plugin)
            : this(plugin, new LoadProgress())
        {}

        protected override void Dispose(bool disposing)
        {
            MinerPluginsManager.InstallRemoveProgress(Plugin.PluginUUID, Progress);
        }

        protected PluginEntryVM(PluginPackageInfoCR plugin, LoadProgress load)
        {
            Plugin = plugin;

            // Filter the dict to remove empty entries
            FilteredSupportedAlgorithms = new Dictionary<string, List<string>>();
            foreach (var kvp in Plugin.SupportedDevicesAlgorithms)
            {
                if (kvp.Value == null || kvp.Value.Count <= 0) continue;
                FilteredSupportedAlgorithms[kvp.Key] = kvp.Value;
            }

            Load = load;
            Load.PropertyChanged += Install_PropertyChanged;

            Progress = new Progress<Tuple<PluginInstallProgressState, int>>(status =>
            {
                var (state, progress) = status;

                string statusText;

                switch (state)
                {
                    case PluginInstallProgressState.Pending:
                        statusText = Tr("Pending Install");
                        break;
                    case PluginInstallProgressState.DownloadingMiner:
                        statusText = Tr("Downloading Miner: {0} %", $"{progress:F2}");
                        break;
                    case PluginInstallProgressState.DownloadingPlugin:
                        statusText = Tr("Downloading Plugin: {0} %", $"{progress:F2}");
                        break;
                    case PluginInstallProgressState.ExtractingMiner:
                        statusText = Tr("Extracting Miner: {0} %", $"{progress:F2}");
                        break;
                    case PluginInstallProgressState.ExtractingPlugin:
                        statusText = Tr("Extracting Plugin: {0} %", $"{progress:F2}");
                        break;
                    default:
                        statusText = Tr("Pending Install");
                        break;
                }

                Load.IsInstalling = state < PluginInstallProgressState.FailedDownloadingPlugin;
                Load.Report((statusText, progress));
            });
            MinerPluginsManager.InstallAddProgress(plugin.PluginUUID, Progress);
        }

        private void Install_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Load.IsInstalling))
                OnPropertyChanged(nameof(InstallButtonEnabled));
        }

        public async Task InstallRemovePlugin()
        {
            if (Load.IsInstalling) return;

            if (Plugin.Installed)
            {
                MinerPluginsManager.Remove(Plugin.PluginUUID);
            }
            else
            {
                await MinerPluginsManager.DownloadAndInstall(Plugin.PluginUUID, Progress);
            }

            OnPropertyChanged(nameof(InstallString));
        }
    }
}
