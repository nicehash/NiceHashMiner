using NHM.Common;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;
using System.Linq;

using InternalConfigsCommon = NHM.Common.Configs.InternalConfigs;

namespace NHM.MinerPluginToolkitV1
{
    static class BenchmarkProcessSettings
    {
        // for internal BenchmarkExceptions
        static BenchmarkLineExceptions _settingsGlobal = new BenchmarkLineExceptions
        {
            BenchmarkLineMessageExceptions = new Dictionary<string, string>
            {
                // ccminer, cpuminer
                { "Cuda error", "CUDA error" },
                { "is not supported", "N/A" },
                { "illegal memory access", "CUDA error" },
                { "unknown error", "Unknown error" },
                { "No servers could be used! Exiting.", "No pools or work can be used for benchmarking" },
                { "Error CL_INVALID_KERNEL", "Error CL_INVALID_KERNEL" },
                { "Error CL_INVALID_KERNEL_ARGS", "Error CL_INVALID_KERNEL_ARGS" },
                // ethminer
                { "No GPU device with sufficient memory was found", "No GPU device with sufficient memory was found." },
                { "Press any key to exit", "Xmr-Stak erred, check its logs" },
                //// generic
                //{ "error", "Unknown error #2" },
                //{"Error", "Unknown error #2" },
            }
        };

        static BenchmarkProcessSettings()
        {
            (_settingsGlobal, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.InternalsPath("GlobalBenchmarkExceptions.json"), _settingsGlobal);
        }

        public static string IsBenchmarkExceptionLine(string line, Dictionary<string, string> localExceptions)
        {
            // check global
            if (_settingsGlobal.BenchmarkLineMessageExceptions != null)
            {
                var foundKey = _settingsGlobal.BenchmarkLineMessageExceptions.Keys.Where(errorString => line.Contains(errorString)).FirstOrDefault();
                if (!string.IsNullOrEmpty(foundKey))
                {
                    return _settingsGlobal.BenchmarkLineMessageExceptions[foundKey];
                }
            }

            // check local
            if (localExceptions != null)
            {
                var foundKey = localExceptions.Keys.Where(errorString => line.Contains(errorString)).FirstOrDefault();
                if (!string.IsNullOrEmpty(foundKey))
                {
                    return localExceptions[foundKey];
                }
            }

            return null;
        }
    }
}
