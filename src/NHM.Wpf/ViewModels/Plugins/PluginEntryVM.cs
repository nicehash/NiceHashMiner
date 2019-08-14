using System.Collections;
using System.Collections.Generic;
using NHM.Wpf.ViewModels.Models;
using NiceHashMiner.Devices;
using NiceHashMiner.Mining.Plugins;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task InstallPlugin()
        {
            // TODO Placeholder

            if (Load.IsInstalling) return;

            Load.IsInstalling = true;

            for (var i = 0d; i <= 100; i += 1)
            {
                Load.Progress = i;
                Load.Status = $"Installing: {i}%";
                await Task.Delay(100);
            }

            Load.IsInstalling = false;
        }
    }
}
