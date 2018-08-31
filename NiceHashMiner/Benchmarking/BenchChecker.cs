using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NiceHashMiner.Benchmarking
{
    internal class BenchChecker
    {
        /// <summary>
        /// Time in seconds after which benchmark is considered finalized
        /// </summary>
        private const double MaxTime = 300;  // 5 min
        /// <summary>
        /// Deviation required to be significant
        /// </summary>
        private const double MinDeviation = 0.05;  // 5%
        
        private readonly List<double> _measuredSpeeds;
        private readonly Stopwatch _stopwatch;

        public BenchChecker()
        {
            _measuredSpeeds = new List<double>();
            _stopwatch = new Stopwatch();
        }

        public void AppendSpeed(double speed)
        {
            _measuredSpeeds.Add(speed);
            _stopwatch.Start();
        }

        public void Stop() => _stopwatch.Stop();

        public BenchDeviationInfo FinalizeIsDeviant(double benchSpeed, double prevWeight)
        {
            Stop();
            // Ignore the first values if they are 0
            var speeds = _measuredSpeeds.SkipWhile(s => s <= 0).ToList();

            if (speeds.Count == 0) return new BenchDeviationInfo(prevWeight);

            var speed = speeds.Average();

            var curWeight = Math.Min(_stopwatch.Elapsed.TotalSeconds / MaxTime, 1);

            if ((benchSpeed <= 0 || Math.Abs(speed / benchSpeed - 1) > MinDeviation) &&
                (curWeight >= prevWeight || prevWeight > 1 || prevWeight < 0))
            {
                return new BenchDeviationInfo(speed, curWeight);
            }

            var newWeight = Math.Min(curWeight + prevWeight, 1);
            return new BenchDeviationInfo(newWeight);
        }
    }
}
