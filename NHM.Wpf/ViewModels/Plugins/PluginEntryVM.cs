using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Wpf.ViewModels.Models.Placeholders;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginEntryVM : BaseVM
    {
        public PluginPackageInfoCR Plugin { get; }

        public bool IsSupported => Plugin.SupportedDevicesAlgorithms.Keys.Contains("NVIDIA");
        public string InstallString => IsSupported ? "Install" : "Not Supported";

        private bool _isInstalling;

        public bool IsInstalling
        {
            get => _isInstalling;
            private set
            {
                _isInstalling = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InstallButtonEnabled));
            }
        }

        public bool InstallButtonEnabled => IsSupported && !IsInstalling;

        private double _installProgress;

        public double InstallProgress
        {
            get => _installProgress;
            private set
            {
                _installProgress = value;
                OnPropertyChanged();
            }
        }

        private string _installStatus;

        public string InstallStatus
        {
            get => _installStatus;
            private set
            {
                _installStatus = value;
                OnPropertyChanged();
            }
        }

        public PluginEntryVM(PluginPackageInfoCR plugin)
        {
            Plugin = plugin;
        }

        public async Task InstallPlugin()
        {
            // TODO Placeholder

            if (IsInstalling) return;

            IsInstalling = true;

            for (var i = 0d; i <= 100; i += 1)
            {
                InstallProgress = i;
                InstallStatus = $"Installing: {i}%";
                await Task.Delay(100);
            }

            IsInstalling = false;
        }
    }
}
