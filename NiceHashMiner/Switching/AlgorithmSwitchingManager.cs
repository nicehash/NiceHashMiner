using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using NiceHashMiner.Configs;
using NiceHashMiner.Enums;

namespace NiceHashMiner.Switching
{
    public class AlgorithmSwitchingManager
    {
        private const string Tag = "SwitchingManager";

        public event EventHandler<SmaUpdateEventArgs> SmaCheck;

        private Timer _smaCheckTimer;
        private readonly Random _random = new Random();

        private int _ticksForStable;
        private int _ticksForUnstable;
        private double _smaCheckTime;

        // Simplify accessing config objects
        private static Interval StableRange => ConfigManager.GeneralConfig.SwitchSmaTicksStable;
        private static Interval UnstableRange => ConfigManager.GeneralConfig.SwitchSmaTicksUnstable;
        private static Interval SmaCheckRange => ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds;

        private static int MaxHistory => Math.Max(StableRange.Upper, UnstableRange.Upper);

        private readonly Dictionary<AlgorithmType, AlgorithmHistory> _stableHistory;
        private readonly Dictionary<AlgorithmType, AlgorithmHistory> _unstableHistory;

        private readonly Dictionary<AlgorithmType, double> _lastLegitPaying;

        public AlgorithmSwitchingManager()
        {
            _stableHistory = new Dictionary<AlgorithmType, AlgorithmHistory>();
            _unstableHistory = new Dictionary<AlgorithmType, AlgorithmHistory>();
            _lastLegitPaying = new Dictionary<AlgorithmType, double>();

            foreach (var kvp in NHSmaData.FilteredCurrentProfits(true))
            {
                _stableHistory[kvp.Key] = new AlgorithmHistory(MaxHistory);
                _lastLegitPaying[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in NHSmaData.FilteredCurrentProfits(false))
            {
                _unstableHistory[kvp.Key] = new AlgorithmHistory(MaxHistory);
                _lastLegitPaying[kvp.Key] = kvp.Value;
            }
        }

        public void Start()
        {
            _smaCheckTimer = new Timer(100);
            _smaCheckTimer.Elapsed += SmaCheckTimerOnElapsed;

            _smaCheckTimer.Start();
        }

        public void Stop()
        {
            _smaCheckTimer.Stop();
            _smaCheckTimer = null;
        }

        private void SmaCheckTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Randomize();

            var sb = new StringBuilder();
            sb.AppendLine("Normalizing profits");

            UpdateProfits(_stableHistory, _ticksForStable, sb);
            UpdateProfits(_unstableHistory, _ticksForUnstable, sb);

            Helpers.ConsolePrint(Tag, sb.ToString());

            var args = new SmaUpdateEventArgs(_lastLegitPaying);
            SmaCheck?.Invoke(this, args);

            _smaCheckTimer.Interval = _smaCheckTime * 1000;
        }

        private void UpdateProfits(Dictionary<AlgorithmType, AlgorithmHistory> history, int ticks, StringBuilder sb)
        {
            foreach (var algo in history.Keys)
            {
                if (NHSmaData.TryGetPaying(algo, out var paying))
                {
                    history[algo].Add(paying);
                    if (paying > _lastLegitPaying[algo])
                    {
                        var i = history[algo].CountOverProfit(_lastLegitPaying[algo]);
                        if (i >= ticks)
                        {
                            _lastLegitPaying[algo] = paying;
                            sb.AppendLine($"\tTAKEN: new profit {paying:e5} after {i} ticks for {algo}");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"\tPOSTPONED: new profit {paying:e5} (previously {_lastLegitPaying[algo]:e5})," +
                                $" higher for {i}/{ticks} ticks for {algo}"
                            );
                        }
                    } 
                    else
                    {
                        // Profit has gone down
                        _lastLegitPaying[algo] = paying;
                    }
                }
            }
        }

        private void Randomize()
        {
            // Lock in case this gets called simultaneously
            // Random breaks down when called from multiple threads
            lock (_random)
            {
                _ticksForStable = StableRange.RandomInt(_random);
                _ticksForUnstable = UnstableRange.RandomInt(_random);
                _smaCheckTime = SmaCheckRange.RandomInt(_random);
            }
        }
    }

    public class SmaUpdateEventArgs : EventArgs
    {
        public readonly Dictionary<AlgorithmType, double> NormalizedProfits;

        public SmaUpdateEventArgs(Dictionary<AlgorithmType, double> profits)
        {
            NormalizedProfits = profits;
        }
    }
}
