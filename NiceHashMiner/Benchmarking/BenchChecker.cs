using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public BenchChecker()
        {
            _measuredSpeeds = new List<double>();
        }

        public void AppendSpeed(double speed)
        {
            _measuredSpeeds.Add(speed);
        }

        public BenchDeviationInfo FinalizeIsDeviant(double benchSpeed, double time, double prevWeight)
        {
            // Ignore the first values if they are 0
            var speed = _measuredSpeeds.SkipWhile(s => s <= 0).Average();
            var curWeight = Math.Min(time / MaxTime, 1);

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
