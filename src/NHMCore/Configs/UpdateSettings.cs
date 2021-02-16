using NHM.Common;

namespace NHMCore.Configs
{
    public class UpdateSettings : NotifyChangedBase
    {
        private static object _lock = new object();

        public static UpdateSettings Instance { get; } = new UpdateSettings();
        private UpdateSettings() { }

        public bool _autoUpdateNiceHashMiner = false;
        public bool AutoUpdateNiceHashMiner
        {
            get
            {
                lock (_lock)
                {
                    return _autoUpdateNiceHashMiner;
                }
            }
            set
            {
                lock (_lock)
                {
                    _autoUpdateNiceHashMiner = value;
                }
                OnPropertyChanged(nameof(AutoUpdateNiceHashMiner));
            }
        }

        public bool _autoUpdateMinerPlugins = true;
        public bool AutoUpdateMinerPlugins
        {
            get
            {
                lock (_lock)
                {
                    return _autoUpdateMinerPlugins;
                }
            }
            set
            {
                lock (_lock)
                {
                    _autoUpdateMinerPlugins = value;
                }
                OnPropertyChanged(nameof(AutoUpdateMinerPlugins));
            }
        }
    }
}
