using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Benchmarking
{
    // temp solution before we fully switch to plugin system
    static class BenchmarkTimes
    {
        public static int GetTime(BenchmarkPerformanceType benchmarkPerformanceType, DeviceType deviceType)
        {
            if (deviceType == DeviceType.AMD)
            {
                switch (benchmarkPerformanceType)
                {
                    case BenchmarkPerformanceType.Quick:
                        return 120;
                    case BenchmarkPerformanceType.Standard:
                        return 180;
                    case BenchmarkPerformanceType.Precise:
                        return 240;
                }
            }

            // NVIDIA
            switch (benchmarkPerformanceType)
            {
                case BenchmarkPerformanceType.Quick:
                    return 20;
                case BenchmarkPerformanceType.Standard:
                    return 60;
                case BenchmarkPerformanceType.Precise:
                    return 120;
            }

            return 40;
        }
    }
}
