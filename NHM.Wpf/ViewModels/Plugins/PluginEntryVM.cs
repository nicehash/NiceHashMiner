using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Wpf.ViewModels.Models;
using NHM.Wpf.ViewModels.Models.Placeholders;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginEntryVM : BaseVM
    {
        public PluginPackageInfoCR Plugin { get; }

        public bool IsSupported => Plugin.SupportedDevicesAlgorithms.Keys.Contains("NVIDIA");
        public string InstallString => IsSupported ? "Install" : "Not Supported";

        public bool InstallButtonEnabled => IsSupported && !Install.IsInstalling;

        public InstallProgress Install { get; }

        public PluginEntryVM(PluginPackageInfoCR plugin)
            : this(plugin, new InstallProgress())
        { }

        public PluginEntryVM(PluginPackageInfoCR plugin, InstallProgress install)
        {
            Plugin = plugin;

            Install = install;
            Install.PropertyChanged += Install_PropertyChanged;
        }

        private void Install_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Install.IsInstalling))
                OnPropertyChanged(nameof(InstallButtonEnabled));
        }

        public async Task InstallPlugin()
        {
            // TODO Placeholder

            if (Install.IsInstalling) return;

            Install.IsInstalling = true;

            for (var i = 0d; i <= 100; i += 1)
            {
                Install.Progress = i;
                Install.Status = $"Installing: {i}%";
                await Task.Delay(100);
            }

            Install.IsInstalling = false;
        }
    }
}
