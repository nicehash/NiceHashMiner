using NHM.Common;
using NHMCore.Mining.Plugins;
using NiceHashMiner.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static NHMCore.Translations;

namespace NiceHashMiner.ViewModels.Plugins
{
    public class PluginEntryVM : BaseVM
    {
        public PluginPackageInfoCR Plugin { get; }
        private IProgress<Tuple<PluginInstallProgressState, int>> Progress;

        public string InstallString
        {
            get
            {
                // order really matters
                if (Load.IsInstalling && Plugin.Supported)
                {
                    if (!Plugin.Installed) return Tr("INSTALLING");
                    if (Plugin.HasNewerVersion) return Tr("UPDATING");
                }

                if (Plugin.HasNewerVersion) return Tr("UPDATE");
                if (Plugin.Installed) return Tr("INSTALLED");
                if (Plugin.Supported) return Tr("INSTALL");
                return Tr("Not Supported");
            }
        }

        public string InstallVersionStatus
        {
            get
            {
                var localVer = Plugin?.LocalInfo?.PluginVersion ?? null;
                var onlineVer = Plugin?.OnlineInfo?.PluginVersion ?? null;
                if (!Plugin.Installed && onlineVer != null)
                {
                    return Tr("{0}.{1} (Online)", onlineVer.Major, onlineVer.Minor);
                }
                if (Plugin.Installed && Plugin.HasNewerVersion && localVer != null && onlineVer != null)
                {
                    return $"{localVer.Major}.{localVer.Minor} / {onlineVer.Major}.{onlineVer.Minor}";
                }
                if (Plugin.Installed && !Plugin.HasNewerVersion && localVer != null && onlineVer != null)
                {
                    return Tr("{0}.{1} (Latest)", onlineVer.Major, onlineVer.Minor);
                }
                if (localVer != null)
                {
                    return Tr("{0}.{1} (Local)", localVer.Major, localVer.Minor);
                }
                return Tr("N/A");
            }
        }

        //public bool InstallButtonEnabled => (Plugin.Installed || Plugin.Supported) && !Load.IsInstalling;
        public bool InstallButtonEnabled => Plugin.Supported && !Load.IsInstalling && (Plugin.HasNewerVersion || !Plugin.Installed);


        public Visibility ActionsButtonVisibility
        {
            get
            {
                if (Load.IsInstalling) return Visibility.Collapsed;
                if (Plugin.Installed) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }
        public Visibility SpinningCircleVisibility
        {
            get
            {
                if (Load.IsInstalling) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        // Load is shared between the listing and detail page. This allows navigating to details and back
        // while maintaining the progress.
        public LoadProgress Load { get; }

        protected readonly Dictionary<string, List<string>> FilteredSupportedAlgorithms;

        public PluginEntryVM(PluginPackageInfoCR plugin)
            : this(plugin, new LoadProgress())
        { }

        protected override void Dispose(bool disposing)
        {
            MinerPluginsManager.InstallRemoveProgress(Plugin.PluginUUID, Progress);
        }

        protected PluginEntryVM(PluginPackageInfoCR plugin, LoadProgress load)
        {
            Plugin = plugin;
            Plugin.PropertyChanged += Plugin_PropertyChanged;

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
                        statusText = Tr("Downloading Miner: {0}%", progress);
                        break;
                    case PluginInstallProgressState.DownloadingPlugin:
                        statusText = Tr("Downloading Plugin: {0}%", progress);
                        break;
                    case PluginInstallProgressState.ExtractingMiner:
                        statusText = Tr("Extracting Miner: {0}%", progress);
                        break;
                    case PluginInstallProgressState.ExtractingPlugin:
                        statusText = Tr("Extracting Plugin: {0}%", progress);
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
            {
                CommonInstallOnPropertyChanged();
            }

        }

        private void Plugin_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Logger.Debug("PluginEntryVM", $"Plugin_PropertyChanged PropertyName: '{e.PropertyName}'");
            if (e.PropertyName == nameof(PluginPackageInfoCR.OnlineInfo) || e.PropertyName == nameof(PluginPackageInfoCR.LocalInfo))
            {
                OnPropertyChanged(nameof(InstallVersionStatus));
            }
            if (e.PropertyName == nameof(PluginPackageInfoCR.HasNewerVersion))
            {
                OnPropertyChanged(nameof(InstallString));
            }
        }

        public async Task InstallOrUpdatePlugin()
        {
            if (Load.IsInstalling) return;
            if (Plugin.Installed && !Plugin.HasNewerVersion) return;
            //if (Plugin.HasNewerVersion) return;

            await MinerPluginsManager.DownloadAndInstall(Plugin.PluginUUID, Progress, CancellationToken.None);
            CommonInstallOnPropertyChanged();
        }

        public async Task UninstallPlugin()
        {
            if (Load.IsInstalling) return;
            if (!Plugin.Installed) return;
            await MinerPluginsManager.RemovePlugin(Plugin.PluginUUID);

            CommonInstallOnPropertyChanged();
        }

        public void ShowPluginInternals()
        {
            var pluginPath = Paths.MinerPluginsPath(Plugin.PluginUUID);
            Process.Start(pluginPath);
        }

        private void CommonInstallOnPropertyChanged()
        {
            OnPropertyChanged(nameof(InstallString));
            OnPropertyChanged(nameof(InstallButtonEnabled));
            OnPropertyChanged(nameof(ActionsButtonVisibility));
            OnPropertyChanged(nameof(SpinningCircleVisibility));
        }

    }
}
