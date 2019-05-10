using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Benchmarking
{
    public class BenchmarkingAnalyzer
    {

        public struct BenchmarkSpeed
        {
            public double primarySpeed { get; set; }
            public double secondarySpeed { get; set; }
        }

        public struct MiningSpeed
        {
            public double primarySpeed { get; set; }
            public double secondarySpeed { get; set; }
            public DateTime time { get; set; } //TODO maybe not needed
        }

        Dictionary<string, BenchmarkSpeed> BenchmarkedSpeeds = new Dictionary<string, BenchmarkSpeed>();

        Dictionary<string, List<MiningSpeed>> MiningSpeeds = new Dictionary<string, List<MiningSpeed>>();

        private const double Threshold = 0.05;

        public void SetBenchmarkSpeeds(string speedID, BenchmarkSpeed speeds)
        {
            if (BenchmarkedSpeeds.Where(t => t.Key == speedID).Count() == 0)
            {
                BenchmarkedSpeeds.Add(speedID, speeds);
            }
            else
            {
                BenchmarkedSpeeds[speedID] = speeds;
            }
        }

        public void UpdateMiningSpeeds(string speedID, MiningSpeed measurements)
        {
            if(MiningSpeeds.Where(t => t.Key == speedID).Count() == 0)
            {
                MiningSpeeds.Add(speedID, new List<MiningSpeed>() { measurements});
            }
            else
            {
                MiningSpeeds[speedID].Add(measurements);
            }
        }

        public void Clear()
        {
            BenchmarkedSpeeds.Clear();
            MiningSpeeds.Clear();
        }

        public bool IsDeviant(string speedID, List<MiningSpeed> measurments)
        {
            var speeds = MiningSpeeds[speedID];
            var primarySpeeds = new List<double>();
            var secondarySpeeds = new List<double>();
            foreach(var kvp in speeds)
            {
                primarySpeeds.Add(kvp.primarySpeed);
                secondarySpeeds.Add(kvp.secondarySpeed);
            }
            return false;
        }
    }
}
