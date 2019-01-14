using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SystemTimer = System.Timers.Timer;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        #region TODO temporary / refactoring
        // TODO temporary
        public static IRatesComunication ratesComunication = null; // 
        #endregion 

        private static SystemTimer _minerStatsCheck;

        private static void StartMinerStatsCheckTimer()
        {
            _minerStatsCheck = new SystemTimer();
            _minerStatsCheck.Elapsed += async (object sender, ElapsedEventArgs e) => {
                await MinersManager.MinerStatsCheck();
            };
            _minerStatsCheck.Interval = ConfigManager.GeneralConfig.MinerAPIQueryInterval * 1000;
            _minerStatsCheck.Start();
        }

        private static void StopMinerStatsCheckTimer()
        {
            _minerStatsCheck.Stop();
            _minerStatsCheck = null;
        }
    }
}
