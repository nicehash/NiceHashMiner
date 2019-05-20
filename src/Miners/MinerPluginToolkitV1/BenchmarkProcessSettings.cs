using MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1
{
    static class BenchmarkProcessSettings
    {
        // for internal BenchmarkExceptions
        static Dictionary<string, string> GlobalBenchmarkExceptions { get; set; } = new Dictionary<string, string>
        {
            // ccminer, cpuminer
            {"Cuda error", "CUDA error" },
            {"is not supported", "N/A" },
            {"illegal memory access", "CUDA error" },
            {"unknown error", "Unknown error" },
            {"No servers could be used! Exiting.", "No pools or work can be used for benchmarking" },
            {"Error CL_INVALID_KERNEL", "Error CL_INVALID_KERNEL" },
            {"Error CL_INVALID_KERNEL_ARGS", "Error CL_INVALID_KERNEL_ARGS" },
            // ethminer
            {"No GPU device with sufficient memory was found", "No GPU device with sufficient memory was found." },
            {"Press any key to exit", "Xmr-Stak erred, check its logs" },
            //// generic
            //{ "error", "Unknown error #2" },
            //{"Error", "Unknown error #2" },

        };

        static BenchmarkProcessSettings()
        {
            const string globalBenchmarkExceptionsPath = @"internals\GlobalBenchmarkExceptions.json";
            var globalBenchmarkExceptions = InternalConfigs.ReadFileSettings<Dictionary<string, string>>(globalBenchmarkExceptionsPath);
            if (globalBenchmarkExceptions != null)
            {
                GlobalBenchmarkExceptions = globalBenchmarkExceptions;
            }
            else
            {
                InternalConfigs.WriteFileSettings(globalBenchmarkExceptionsPath, GlobalBenchmarkExceptions);
            }
        }

        public static string IsBenchmarkExceptionLine(string line, Dictionary<string, string> localExceptions)
        {
            // check global
            if (GlobalBenchmarkExceptions != null)
            {
                var foundKey = GlobalBenchmarkExceptions.Keys.Where(errorString => line.Contains(errorString)).FirstOrDefault();
                if (!string.IsNullOrEmpty(foundKey))
                {
                    return GlobalBenchmarkExceptions[foundKey];
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
