using NHM.Common;
using NHMCore.Nhmws.V4;
using System.Threading.Tasks;

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
                _ = Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
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
                _ = Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
            }
        }
    }
}
