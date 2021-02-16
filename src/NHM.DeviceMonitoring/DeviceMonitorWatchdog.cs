using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorWatchdog
    {
        private DateTime? _lastReportedError = null;
        private List<TimeSpan> _restartTimeouts = new List<TimeSpan>();
        private int _restartTimeoutIndex = 0;
        private object _lock = new object();

        internal DeviceMonitorWatchdog(params TimeSpan[] restartTimeouts)
        {
            if (restartTimeouts.Count() == 0)
            {
                Environment.FailFast("restartTimeouts MUST have at least one value");
            }
            foreach (var ts in restartTimeouts) _restartTimeouts.Add(ts);
        }

        internal bool IsAttemptErrorRecoveryPermanentlyDisabled()
        {
            lock (_lock)
            {
                var permanentDisable = _restartTimeoutIndex >= _restartTimeouts.Count;
                return permanentDisable;
            }
        }

        internal void AppendTimeoutTimeSpan(TimeSpan restartTimeout)
        {
            if (IsAttemptErrorRecoveryPermanentlyDisabled()) return;
            _restartTimeouts.Add(restartTimeout);
        }

        internal void SetErrorTime()
        {
            lock (_lock)
            {
                if (IsAttemptErrorRecoveryPermanentlyDisabled()) return;
                if (!_lastReportedError.HasValue) _lastReportedError = DateTime.UtcNow;
            }
        }

        internal void UpdateTickError()
        {
            lock (_lock)
            {
                if (IsAttemptErrorRecoveryPermanentlyDisabled()) return;
                _lastReportedError = DateTime.UtcNow;
                _restartTimeoutIndex++;
            }
        }

        internal bool ShouldAttemptErrorRecovery()
        {
            lock (_lock)
            {
                if (IsAttemptErrorRecoveryPermanentlyDisabled()) return false;
                if (!_lastReportedError.HasValue) return false;
                if (_restartTimeoutIndex < 0) return false;
                var restartTimeout = _restartTimeouts[_restartTimeoutIndex];
                var elapsed = DateTime.UtcNow - _lastReportedError.Value;
                var attemptRecovery = elapsed >= restartTimeout;
                return attemptRecovery;
            }
        }

        internal void Reset()
        {
            lock (_lock)
            {
                if (IsAttemptErrorRecoveryPermanentlyDisabled()) return;
                _lastReportedError = null;
                _restartTimeoutIndex = 0;
            }
        }
    }
}
