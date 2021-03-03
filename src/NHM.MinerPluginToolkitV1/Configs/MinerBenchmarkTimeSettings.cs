using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Configs;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1.Configs
{
    public class MinerBenchmarkTimeSettings : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("max_ticks_enabled")]
        public bool MaxTicksEnabled { get; set; } = true;

        [JsonProperty("time_general_s")]
        public Dictionary<BenchmarkPerformanceType, int> General { get; set; } = new Dictionary<BenchmarkPerformanceType, int> {
            { BenchmarkPerformanceType.Quick, 20 },
            { BenchmarkPerformanceType.Standard, 60 },
            { BenchmarkPerformanceType.Precise, 120 },
        };

        [JsonProperty("time_per_algorithm_s")]
        public Dictionary<BenchmarkPerformanceType, Dictionary<string, int>> PerAlgorithm { get; set; } = new Dictionary<BenchmarkPerformanceType, Dictionary<string, int>> {
            { BenchmarkPerformanceType.Quick, new Dictionary<string, int>() },
            { BenchmarkPerformanceType.Standard, new Dictionary<string, int>() },
            { BenchmarkPerformanceType.Precise, new Dictionary<string, int>() },
        };

        [JsonProperty("max_ticks_general")]
        public Dictionary<BenchmarkPerformanceType, int> GeneralTicks { get; set; } = new Dictionary<BenchmarkPerformanceType, int> {
            { BenchmarkPerformanceType.Quick, 1 },
            { BenchmarkPerformanceType.Standard, 3 },
            { BenchmarkPerformanceType.Precise, 9 },
        };

        [JsonProperty("max_ticks_per_algorithm")]
        public Dictionary<BenchmarkPerformanceType, Dictionary<string, int>> PerAlgorithmTicks { get; set; } = new Dictionary<BenchmarkPerformanceType, Dictionary<string, int>> {
            { BenchmarkPerformanceType.Quick, new Dictionary<string, int>() },
            { BenchmarkPerformanceType.Standard, new Dictionary<string, int>() },
            { BenchmarkPerformanceType.Precise, new Dictionary<string, int>() },
        };

        public static int ParseBenchmarkTime(List<int> defaults, MinerBenchmarkTimeSettings timeSetting, IEnumerable<MiningPair> miningPairs, BenchmarkPerformanceType benchmarkType)
        {
            var dict = new Dictionary<BenchmarkPerformanceType, int> {
                {BenchmarkPerformanceType.Quick, defaults[0] },
                {BenchmarkPerformanceType.Standard, defaults[1] },
                {BenchmarkPerformanceType.Precise, defaults[2] },
            };
            return ParseBenchmarkTime(dict, timeSetting, miningPairs, benchmarkType);
        }

        public static int ParseBenchmarkTime(Dictionary<BenchmarkPerformanceType, int> defaults, MinerBenchmarkTimeSettings timeSetting, IEnumerable<MiningPair> miningPairs, BenchmarkPerformanceType benchmarkType)
        {
            try
            {
                if (timeSetting?.UseUserSettings ?? false)
                {
                    // TimePerAlgorithm has #1 priority
                    if (timeSetting.PerAlgorithm != null && timeSetting.PerAlgorithm.ContainsKey(benchmarkType))
                    {
                        var pairTypeTimeout = timeSetting.PerAlgorithm[benchmarkType];
                        var algorithmName = miningPairs.FirstOrDefault()?.Algorithm?.AlgorithmName ?? "";
                        if (pairTypeTimeout != null && !string.IsNullOrEmpty(algorithmName) && pairTypeTimeout.ContainsKey(algorithmName))
                        {
                            return pairTypeTimeout[algorithmName];
                        }
                    }
                    // TimePerType has #2 priority
                    return timeSetting.General[benchmarkType];
                }
            }
            catch (Exception e)
            {
                Logger.Error("ParseBenchmarkTime", $"ParseBenchmarkTime failed: {e.Message}");
            }

            return defaults[benchmarkType];
        }

        public static int ParseBenchmarkTicks(List<int> defaults, MinerBenchmarkTimeSettings timeSetting, IEnumerable<MiningPair> miningPairs, BenchmarkPerformanceType benchmarkType)
        {
            var dict = new Dictionary<BenchmarkPerformanceType, int> {
                {BenchmarkPerformanceType.Quick, defaults[0] },
                {BenchmarkPerformanceType.Standard, defaults[1] },
                {BenchmarkPerformanceType.Precise, defaults[2] },
            };
            return ParseBenchmarkTicks(dict, timeSetting, miningPairs, benchmarkType);
        }

        public static int ParseBenchmarkTicks(Dictionary<BenchmarkPerformanceType, int> defaults, MinerBenchmarkTimeSettings timeSetting, IEnumerable<MiningPair> miningPairs, BenchmarkPerformanceType benchmarkType)
        {
            try
            {
                if (timeSetting?.UseUserSettings ?? false)
                {
                    // TimePerAlgorithm has #1 priority
                    if (timeSetting.PerAlgorithmTicks != null && timeSetting.PerAlgorithmTicks.ContainsKey(benchmarkType))
                    {
                        var pairTypeTimeout = timeSetting.PerAlgorithmTicks[benchmarkType];
                        var algorithmName = miningPairs.FirstOrDefault()?.Algorithm?.AlgorithmName ?? "";
                        if (pairTypeTimeout != null && !string.IsNullOrEmpty(algorithmName) && pairTypeTimeout.ContainsKey(algorithmName))
                        {
                            return pairTypeTimeout[algorithmName];
                        }
                    }
                    // TimePerType has #2 priority
                    return timeSetting.GeneralTicks[benchmarkType];
                }
            }
            catch (Exception e)
            {
                Logger.Error("ParseBenchmarkTicks", $"ParseBenchmarkTicks failed: {e.Message}");
            }

            return defaults[benchmarkType];
        }
    }
}
