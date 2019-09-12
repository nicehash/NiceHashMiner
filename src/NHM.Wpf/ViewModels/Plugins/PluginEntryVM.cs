using NHM.Wpf.ViewModels.Models;
using NHMCore.Mining.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using NHMCore;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginEntryVM : BaseVM
    {
        public PluginPackageInfoCR Plugin { get; }

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
        { }

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
                await InstallOrUpdateAsync();
            }

            OnPropertyChanged(nameof(InstallString));
        }

        private async Task InstallOrUpdateAsync()
        {
            var progressConverter = new Progress<Tuple<PluginInstallProgressState, int>>(status =>
            {
                var (state, progress) = status;

                string statusText;

                switch (state)
                {
                    case PluginInstallProgressState.DownloadingMiner:
                        statusText = $"Downloading Miner: {progress:F2} %";
                        break;

                    case PluginInstallProgressState.DownloadingPlugin:
                        statusText = $"Downloading Plugin: {progress:F2} %";
                        break;

                    case PluginInstallProgressState.ExtractingMiner:
                        statusText = $"Extracting Miner: {progress:F2} %";
                        break;

                    case PluginInstallProgressState.ExtractingPlugin:
                        statusText = $"Extracting Plugin: {progress:F2} %";
                        break;
                    default:
                        statusText = "";
                        break;
                }

                Load.Report((statusText, progress));
            });

            Load.IsInstalling = true;

            await MinerPluginsManager.DownloadAndInstall(Plugin, progressConverter, default);

            Load.IsInstalling = false;
        }
    }
}
