using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Switching
{
    public class AlgorithmSwitchingManager
    {
        public EventHandler SmaCheck;

        private readonly Timer _smaCheckTimer;
        private readonly Random _random = new Random();

        private int _ticksForStable;
        private int _ticksForUnstable;
        private double _smaCheckTime;

        // Simplify accessing config objects
        private Interval _stableRange => ConfigManager.GeneralConfig.SwitchSmaTicksStable;
        private Interval _unstableRange => ConfigManager.GeneralConfig.SwitchSmaTicksUnstable;
        private Interval _smaCheckRange => ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds;

        public AlgorithmSwitchingManager()
        {
            _smaCheckTimer = new Timer(100);
            _smaCheckTimer.Elapsed += SmaCheckTimerOnElapsed;

            _smaCheckTimer.Start();
        }

        private void SmaCheckTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Randomize();

            SmaCheck?.Invoke(this, EventArgs.Empty);
        }

        private void Randomize()
        {
            // In case this gets called simultaneously, since Random breaks down when called from multiple threads
            lock (_random)
            {
                _ticksForStable = _stableRange.RandomInt(_random);
                _ticksForUnstable = _unstableRange.RandomInt(_random);
                _smaCheckTime = _smaCheckRange.RandomInt(_random);
            }
        }
    }
}
