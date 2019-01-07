using System;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Configs.Data
{
    /// <summary>
    /// BenchmarkTimeLimitsConfig is used to set the time limits for benchmarking.
    /// There are three types: Quick, Standard,Precise (look at BenchmarkType.cs).
    /// </summary>
    /// 
    [Serializable]
    public class BenchmarkTimeLimitsConfig
    {
        #region CONSTANTS

        [field: NonSerialized] private static readonly int[] DefaultCpuNvidia = {20, 60, 120};
        [field: NonSerialized] private static readonly int[] DefaultAmd = {120, 180, 240};
        [field: NonSerialized] public static readonly int Size = 3;

        #endregion CONSTANTS

        #region PRIVATES

        private int[] _benchmarkTimeLimitsCpu = MemoryHelper.DeepClone(DefaultCpuNvidia);
        private int[] _benchmarkTimeLimitsNvidia = MemoryHelper.DeepClone(DefaultCpuNvidia);
        private int[] _benchmarkTimeLimitsAmd = MemoryHelper.DeepClone(DefaultAmd);

        private static bool IsValid(int[] value)
        {
            return value != null && value.Length == Size;
        }

        #endregion PRIVATES

        #region PROPERTIES

        public int[] CPU
        {
            get => _benchmarkTimeLimitsCpu;
            set => _benchmarkTimeLimitsCpu = MemoryHelper.DeepClone(IsValid(value) ? value : DefaultCpuNvidia);
        }

        public int[] NVIDIA
        {
            get => _benchmarkTimeLimitsNvidia;
            set => _benchmarkTimeLimitsNvidia = MemoryHelper.DeepClone(IsValid(value) ? value : DefaultCpuNvidia);
        }

        public int[] AMD
        {
            get => _benchmarkTimeLimitsAmd;
            set => _benchmarkTimeLimitsAmd = MemoryHelper.DeepClone(IsValid(value) ? value : DefaultAmd);
        }

        #endregion PROPERTIES

        public int GetBenchamrktime(BenchmarkPerformanceType benchmarkPerformanceType, DeviceGroupType deviceGroupType)
        {
            if (deviceGroupType == DeviceGroupType.CPU)
            {
                return CPU[(int) benchmarkPerformanceType];
            }
            if (deviceGroupType == DeviceGroupType.AMD_OpenCL)
            {
                return AMD[(int) benchmarkPerformanceType];
            }

            return NVIDIA[(int) benchmarkPerformanceType];
        }
    }
}
