using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                MinerPluginToolkitV1.MinerToolkit.HideMiningWindows = value;
                OnPropertyChanged(nameof(HideMiningWindows));
            }
        }

        private bool _minimizeMiningWindows = false;
        public bool MinimizeMiningWindows
        {
            get => _minimizeMiningWindows;
            set
            {
                _minimizeMiningWindows = value;
                MinerPluginToolkitV1.MinerToolkit.MinimizeMiningWindows = value;
                OnPropertyChanged(nameof(MinimizeMiningWindows));
            }
        }

        private int _minerRestartDelayMS = 1000;
        public int MinerRestartDelayMS
        {
            get => _minerRestartDelayMS;
            set
            {
                _minerRestartDelayMS = value;
                MinerPluginToolkitV1.MinerToolkit.MinerRestartDelayMS = value;
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
                MinerPluginToolkitV1.FreePortsCheckerManager.ApiBindPortPoolStart = value;
                OnPropertyChanged(nameof(ApiBindPortPoolStart));
            }
        }
    }
}
