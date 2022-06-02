using NHM.Common;
using NHMCore.Mining;
using NHMCore.Mining.Plugins;
using NHMCore.Utils;
using System.Collections.Generic;

namespace NHMCore.Configs
{
    public class MiscSettings : NotifyChangedBase
    {
        public static MiscSettings Instance { get; } = new MiscSettings();
        private MiscSettings() { }

        private bool _useOptimizationProfiles = false;
        public bool UseOptimizationProfiles
        {
            get => _useOptimizationProfiles;
            set
            {
                _useOptimizationProfiles = value;
                GPUProfileManager.Instance.ServiceEnabled = value;
                OnPropertyChanged(nameof(UseOptimizationProfiles));
            }
        }

        private bool _allowMultipleInstances = true;
        public bool AllowMultipleInstances
        {
            get => _allowMultipleInstances;
            set
            {
                _allowMultipleInstances = value;
                OnPropertyChanged(nameof(AllowMultipleInstances));
            }
        }

        private Dictionary<string, bool> _showNotification = new Dictionary<string, bool>();
        public Dictionary<string, bool> ShowNotifications
        {
            get => _showNotification;
            set
            {
                _showNotification = value;
                OnPropertyChanged(nameof(ShowNotifications));
            }
        }

        public bool CoolDownCheckEnabled
        {
            get => MinerApiWatchdog.Enabled;
            set
            {
                MinerApiWatchdog.Enabled = value;
                OnPropertyChanged(nameof(CoolDownCheckEnabled));
            }
        }

        public bool RunAtStartup
        {
            get => Configs.RunAtStartup.Instance.Enabled;
            set
            {
                Configs.RunAtStartup.Instance.Enabled = value;
                OnPropertyChanged(nameof(RunAtStartup));
            }
        }

        private bool _disableVisualCRedistributableCheck = false;
        public bool DisableVisualCRedistributableCheck
        {
            get => _disableVisualCRedistributableCheck;
            set
            {
                _disableVisualCRedistributableCheck = value;
                OnPropertyChanged(nameof(DisableVisualCRedistributableCheck));
            }
        }

        public bool ResolveNiceHashDomainsToIPs
        {
            get => StratumServiceHelpers.UseDNSQ;
            set
            {
                StratumServiceHelpers.UseDNSQ = value;
                OnPropertyChanged(nameof(ResolveNiceHashDomainsToIPs));
            }
        }
    }
}
