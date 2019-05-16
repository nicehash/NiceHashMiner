using System;
using System.Collections.Generic;
using System.Linq;

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

        public static Dictionary<string, BenchmarkSpeed> BenchmarkedSpeeds = new Dictionary<string, BenchmarkSpeed>();

        public static Dictionary<string, List<MiningSpeed>> MiningSpeeds = new Dictionary<string, List<MiningSpeed>>();

        private const double StabilityThreshold = 0.05;

        public static void SetBenchmarkSpeeds(string speedID, BenchmarkSpeed speeds)
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

        public static void UpdateMiningSpeeds(string speedID, MiningSpeed measurements)
        {
            if (MiningSpeeds.Where(t => t.Key == speedID).Count() == 0)
            {
                MiningSpeeds.Add(speedID, new List<MiningSpeed>() { measurements });
            }
            else
            {
                if(MiningSpeeds[speedID].Count() == 120) //this is 10 min of speed data comming every 5 seconds
                {
                    MiningSpeeds[speedID].RemoveAt(0);
                }
                MiningSpeeds[speedID].Add(measurements);
            }
        }

        public void Clear()
        {
            BenchmarkedSpeeds.Clear();
            MiningSpeeds.Clear();
        }

        public static bool IsDeviant(string speedID)
        {
            var speeds = MiningSpeeds[speedID];
            var primarySpeeds = new List<double>();
            var secondarySpeeds = new List<double>();
            foreach (var kvp in speeds)
            {
                if(DateTime.Compare(kvp.time, DateTime.Now.AddMinutes(-30)) > 0) //if speed is older than 30 minutes we won't use it in calculations
                {
                    if (kvp.primarySpeed != 0) primarySpeeds.Add(kvp.primarySpeed);
                    if (kvp.secondarySpeed != 0) secondarySpeeds.Add(kvp.secondarySpeed);
                }
            }

            //primary speeds
            var deviantPrimary = CheckIfDeviant(primarySpeeds);

            //secondary speeds
            if(secondarySpeeds.Count() != 0)
            {
                var deviantSecondary = CheckIfDeviant(secondarySpeeds);
                return deviantPrimary && deviantSecondary;
            }

            return deviantPrimary;
        }

        public static double CalcAverageSpeed(List<double> speeds)
        {
            var avg = 0.0;
            for(int i=0; i<speeds.Count(); i++)
            {
                avg += speeds[i];
            }
            return avg / speeds.Count();
        } 

        public static List<double> NormalizedStandardDeviation(List<double> speeds)
        {
            var normalizedSpeeds = new List<double>();
            if (speeds.Count() == 0) return normalizedSpeeds;

            var meanSpeed = CalcAverageSpeed(speeds);

            var sumSpeedDiffSquare = new List<double>();
            var standardDeviationSpeed = new List<double>();

            //sum speed diff squares
            for(int i=0; i< speeds.Count(); i++)
            {
                sumSpeedDiffSquare.Add(Math.Pow(speeds[i] - meanSpeed, 2));
            }

            //sqrt
            for(int i=0; i< sumSpeedDiffSquare.Count(); i++)
            {
                standardDeviationSpeed.Add(Math.Sqrt(sumSpeedDiffSquare[i] / speeds.Count()));
            }

            for(int i=0; i<standardDeviationSpeed.Count(); i++)
            {
                normalizedSpeeds.Add(standardDeviationSpeed[i] / meanSpeed);
            }

            return normalizedSpeeds;
        }

        public static bool CheckIfDeviant(List<double> speeds)
        {
            var normalizedSpeeds = NormalizedStandardDeviation(speeds);
            if (normalizedSpeeds.Count() == 0) return false;

            for (int i = 0; i < normalizedSpeeds.Count(); i++)
            {
                if (normalizedSpeeds[i] > StabilityThreshold) return false;
            }

            return true;
        }
    }
}
