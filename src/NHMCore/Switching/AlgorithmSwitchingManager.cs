using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace NHMCore.Switching
{
    /// <summary>
    /// Handles profit switching within a mining session
    /// </summary>
    public class AlgorithmSwitchingManager
    {
        private const string Tag = "SwitchingManager";

        /// <summary>
        /// Emitted when the profits are checked
        /// </summary>
        public event EventHandler<SmaUpdateEventArgs> SmaCheck;

        private Timer _smaCheckTimer;
        private readonly Random _random = new Random();

        private int _ticksForStable;
        private int _ticksForUnstable;
        private double _smaCheckTime = 1;

        // Simplify accessing config objects
        public static Interval StableRange => SwitchSettings.Instance.SwitchSmaTicksStable;
        public static Interval UnstableRange => SwitchSettings.Instance.SwitchSmaTicksUnstable;
        public static Interval SmaCheckRange => SwitchSettings.Instance.SwitchSmaTimeChangeSeconds;

        public static int MaxHistory => Math.Max(StableRange.Upper, UnstableRange.Upper);

        private readonly Dictionary<AlgorithmType, AlgorithmHistory> _stableHistory;
        private readonly Dictionary<AlgorithmType, AlgorithmHistory> _unstableHistory;

        private bool _hasStarted;

        /// <summary>
        /// Currently used normalized profits
        /// </summary>
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
            _smaCheckTimer = new Timer(_smaCheckTime * 1000);
            _smaCheckTimer.Elapsed += SmaCheckTimerOnElapsed;

            _smaCheckTimer.Start();
        }

        public void Stop()
        {
            _smaCheckTimer?.Stop();
            _smaCheckTimer = null;
        }

        public void ForceUpdate()
        {
            var isAllZeroPaying = _lastLegitPaying.Values.Any(paying => paying == 0);
            if (isAllZeroPaying)
            {
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
            var args = new SmaUpdateEventArgs(_lastLegitPaying);
            Stop();
            SmaCheck?.Invoke(this, args);
            Start();
        }

        /// <summary>
        /// Checks profits and updates normalization based on ticks
        /// </summary>
        internal void SmaCheckTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Randomize();

            // Will be null if manually called (in tests)
            if (_smaCheckTimer != null)
                _smaCheckTimer.Interval = _smaCheckTime * 1000;

            var sb = new StringBuilder();

            if (_hasStarted)
            {
                sb.AppendLine("Normalizing profits");
            }

            var stableUpdated = UpdateProfits(_stableHistory, _ticksForStable, sb);
            var unstableUpdated = UpdateProfits(_unstableHistory, _ticksForUnstable, sb);

            if (!stableUpdated && !unstableUpdated && _hasStarted)
            {
                sb.AppendLine("No algos affected (either no SMA update or no algos higher");
            }

            if (_hasStarted)
            {
                Logger.Info(Tag, sb.ToString());
            }
            else
            {
                _hasStarted = true;
            }

            var args = new SmaUpdateEventArgs(_lastLegitPaying);
            SmaCheck?.Invoke(this, args);
        }

        /// <summary>
        /// Check profits for a history dict and update if profit has been higher for required ticks or if it is lower
        /// </summary>
        /// <returns>True iff any profits were postponed or updated</returns>
        private bool UpdateProfits(Dictionary<AlgorithmType, AlgorithmHistory> history, int ticks, StringBuilder sb)
        {
            var updated = false;

            foreach (var algo in history.Keys)
            {
                if (NHSmaData.TryGetPaying(algo, out var paying))
                {
                    history[algo].Add(paying);
                    if (paying > _lastLegitPaying[algo])
                    {
                        updated = true;
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

            return updated;
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

        #region Test methods

        internal double LastPayingForAlgo(AlgorithmType algo)
        {
            return _lastLegitPaying[algo];
        }

        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// Event args used for reporting fresh normalized profits
    /// </summary>
    public class SmaUpdateEventArgs : EventArgs
    {
        public readonly Dictionary<AlgorithmType, double> NormalizedProfits;

        public SmaUpdateEventArgs(Dictionary<AlgorithmType, double> profits)
        {
            NormalizedProfits = profits;
        }
    }
}
