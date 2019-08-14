using NHM.Wpf.ViewModels.Models;
using NiceHashMiner.Mining.Plugins;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginEntryVM : BaseVM
    {
        public PluginPackageInfoCR Plugin { get; }

        public bool IsSupported => Plugin.SupportedDevicesAlgorithms.Keys.Contains("NVIDIA");
        public string InstallString => IsSupported ? "Install" : "Not Supported";

        public bool InstallButtonEnabled => IsSupported && !Load.IsInstalling;

        public LoadProgress Load { get; }

        public PluginEntryVM(PluginPackageInfoCR plugin)
            : this(plugin, new LoadProgress())
        { }

        public PluginEntryVM(PluginPackageInfoCR plugin, LoadProgress load)
        {
            Plugin = plugin;

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
