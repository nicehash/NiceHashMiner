using NHM.Common;

namespace NHMCore.Configs
{
    public class MiningSettings : NotifyChangedBase
    {
        public static MiningSettings Instance { get; } = new MiningSettings();
        private MiningSettings() { }

        private bool _nVIDIAP0State = false;
        public bool NVIDIAP0State
        {
            get => _nVIDIAP0State;
            set
            {
                _nVIDIAP0State = value;
                OnPropertyChanged(nameof(NVIDIAP0State));
            }
        }

        private bool _autoStartMining = false;
        public bool AutoStartMining
        {
            get => _autoStartMining;
            set
            {
                _autoStartMining = value;
                OnPropertyChanged(nameof(AutoStartMining));
            }
        }

        private int _minerAPIQueryInterval = 5;
        public int MinerAPIQueryInterval
        {
            get => _minerAPIQueryInterval;
            set
            {
                _minerAPIQueryInterval = value;
                OnPropertyChanged(nameof(MinerAPIQueryInterval));
            }
        }

        private bool _hideMiningWindows = false;
        public bool HideMiningWindows
        {
            get => _hideMiningWindows;
            set
            {
                _hideMiningWindows = value;
                NHM.MinerPluginToolkitV1.MinerToolkit.HideMiningWindows = value;
                OnPropertyChanged(nameof(HideMiningWindows));
                OnPropertyChanged(nameof(HideMiningWindowsAlertVisible));
            }
        }

        private bool _minimizeMiningWindows = false;
        public bool MinimizeMiningWindows
        {
            get => _minimizeMiningWindows;
            set
            {
                _minimizeMiningWindows = value;
                NHM.MinerPluginToolkitV1.MinerToolkit.MinimizeMiningWindows = value;
                OnPropertyChanged(nameof(MinimizeMiningWindows));
                OnPropertyChanged(nameof(HideMiningWindowsAlertVisible));
            }
        }

        private int _minerRestartDelayMS = 1000;
        public int MinerRestartDelayMS
        {
            get => _minerRestartDelayMS;
            set
            {
                _minerRestartDelayMS = value;
                NHM.MinerPluginToolkitV1.MinerToolkit.MinerRestartDelayMS = value;
                OnPropertyChanged(nameof(MinerRestartDelayMS));
            }
        }

        private int _apiBindPortPoolStart = 4000;
        public int ApiBindPortPoolStart
        {
            get => _apiBindPortPoolStart;
            set
            {
                _apiBindPortPoolStart = value;
                NHM.MinerPluginToolkitV1.FreePortsCheckerManager.ApiBindPortPoolStart = value;
                OnPropertyChanged(nameof(ApiBindPortPoolStart));
            }
        }


        public bool HideMiningWindowsAlertVisible => MinimizeMiningWindows && HideMiningWindows;
    }
}
