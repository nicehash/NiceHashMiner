using System;

namespace NHMCore.Utils
{
    internal class ElapsedTimeChecker
    {
        private Func<TimeSpan> _elapsedTimeSpanInterval;
        private DateTime _lastElapsedDateTime;
        public ElapsedTimeChecker(TimeSpan elapsedTimeSpanInterval, bool startFromMinValue) : this(() => elapsedTimeSpanInterval, startFromMinValue)
        { }

        public ElapsedTimeChecker(Func<TimeSpan> elapsedTimeSpanIntervalFunc, bool startFromMinValue)
        {
            _elapsedTimeSpanInterval = elapsedTimeSpanIntervalFunc;
            if (startFromMinValue)
            {
                _lastElapsedDateTime = DateTime.MinValue;
            }
            else
            {
                _lastElapsedDateTime = DateTime.UtcNow;
            }
        }

        public bool CheckAndMarkElapsedTime()
        {
            var elapsedTime = DateTime.UtcNow - _lastElapsedDateTime;
            var timeToElapse = _elapsedTimeSpanInterval();
            if (elapsedTime > timeToElapse)
            {
                _lastElapsedDateTime = DateTime.UtcNow;
                return true;
            }
            return false;
        }
    }
}
