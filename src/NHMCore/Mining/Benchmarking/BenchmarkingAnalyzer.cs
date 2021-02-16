using System;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Mining.Benchmarking
{
    internal class BenchmarkingAnalyzer
    {
        public struct BenchmarkSpeed
        {
            public double PrimarySpeed { get; set; }
            public double SecondarySpeed { get; set; }
        }

        public struct MiningSpeed
        {
            public double PrimarySpeed { get; set; }
            public double SecondarySpeed { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public static Dictionary<string, BenchmarkSpeed> BenchmarkedSpeeds = new Dictionary<string, BenchmarkSpeed>();

        public static Dictionary<string, List<MiningSpeed>> MiningSpeeds = new Dictionary<string, List<MiningSpeed>>();

        private const double StabilityThreshold = 0.05;

        private const int MaxHistoryTimeRangeInMinutes = 10;

        /// <summary>
        /// This is a minimum number of <see cref="MiningSpeed"/> elements, required for deviant checking
        /// Each element is generated every 5 seconds, therefore 36 is 3 minutes of data
        /// </summary>
        private static int DeviantCap => (60 / Configs.MiningSettings.Instance.MinerAPIQueryInterval) * 3;

        /// <summary>
        /// This is our max number of <see cref="MiningSpeed"/> elements that we use to analyze.
        /// Equals 10 min of speed data comming every 5 seconds.
        /// </summary>
        private static int MaxHistory => (MaxHistoryTimeRangeInMinutes * 60) / Configs.MiningSettings.Instance.MinerAPIQueryInterval;

        // SpeedID => {DeviceUUID}-{AlgorithmUUID}

        public static void SetBenchmarkSpeeds(string speedID, BenchmarkSpeed speeds)
        {
            BenchmarkedSpeeds[speedID] = speeds;
        }

        public static void UpdateMiningSpeeds(string speedID, MiningSpeed measurements)
        {
            if (!MiningSpeeds.ContainsKey(speedID))
            {
                MiningSpeeds[speedID] = new List<MiningSpeed>();
            }

            if (MiningSpeeds[speedID].Count() == MaxHistory)
            {
                MiningSpeeds[speedID].RemoveAt(0);
            }
            MiningSpeeds[speedID].Add(measurements);
        }

        public static List<double> GetStableSpeeds(string speedID)
        {
            // TODO this could be problematic
            if (!MiningSpeeds.ContainsKey(speedID)) return null;

            var speeds = MiningSpeeds[speedID];
            var primarySpeeds = speeds.Select(miningSpeed => miningSpeed.PrimarySpeed).ToList();
            var secondarySpeeds = speeds.Select(miningSpeed => miningSpeed.SecondarySpeed).ToList();
            var averagedPrimarySpeed = CalcAverageSpeed(primarySpeeds);
            var averagedSecondarySpeeds = CalcAverageSpeed(secondarySpeeds);

            return new List<double> { averagedPrimarySpeed, averagedSecondarySpeeds };
        }

        public void Clear()
        {
            //BenchmarkedSpeeds.Clear();
            //MiningSpeeds.Clear();
            Clear(BenchmarkedSpeeds);
            Clear(MiningSpeeds);
        }

        private void Clear(Dictionary<string, BenchmarkSpeed> benchmarkedSpeeds)
        {
            benchmarkedSpeeds.Clear();
        }
        private void Clear(Dictionary<string, List<MiningSpeed>> miningSpeed)
        {
            miningSpeed.Clear();
        }

        public static bool IsDeviant(string speedID)
        {
            if (!MiningSpeeds.ContainsKey(speedID)) return false;
            if (MiningSpeeds[speedID].Count() < DeviantCap) return false;

            var speeds = MiningSpeeds[speedID];
            var primarySpeeds = new List<double>();
            var secondarySpeeds = new List<double>();

            var timestamps = speeds.Select(speedMeassurment => speedMeassurment.Timestamp).ToList();

            var timestampDiffs = timestamps.Zip(timestamps.GetRange(1, timestamps.Count - 1), (first, second) => second.Subtract(first) > TimeSpan.FromMinutes(2));
            if (timestampDiffs.Any(thresholdExceded => thresholdExceded)) return false;

            foreach (var kvp in speeds)
            {
                if (kvp.PrimarySpeed != 0) primarySpeeds.Add(kvp.PrimarySpeed);
                if (kvp.SecondarySpeed != 0) secondarySpeeds.Add(kvp.SecondarySpeed);
            }

            //primary speeds
            var deviantPrimary = CheckIfDeviant(primarySpeeds);

            //secondary speeds
            if (secondarySpeeds.Count() != 0)
            {
                var deviantSecondary = CheckIfDeviant(secondarySpeeds);
                return deviantPrimary && deviantSecondary;
            }

            return deviantPrimary;
        }

        public static double CalcAverageSpeed(List<double> speeds)
        {
            if (speeds == null || speeds.Count() == 0) return 0d;

            var avg = 0.0;
            for (int i = 0; i < speeds.Count(); i++)
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
            for (int i = 0; i < speeds.Count(); i++)
            {
                sumSpeedDiffSquare.Add(Math.Pow(speeds[i] - meanSpeed, 2));
            }

            //sqrt
            for (int i = 0; i < sumSpeedDiffSquare.Count(); i++)
            {
                standardDeviationSpeed.Add(Math.Sqrt(sumSpeedDiffSquare[i] / speeds.Count()));
            }

            for (int i = 0; i < standardDeviationSpeed.Count(); i++)
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
