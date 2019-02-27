using System.Collections.Generic;

namespace NiceHashMiner.Devices.Querying
{
    internal static class CpuQuery
    {
        private const string Tag = "QueryCPU";

        public static List<CpuComputeDevice> QueryCpus(out bool failed64Bit, out bool failedCpuCount)
        {
            Helpers.ConsolePrint(Tag, "QueryCpus START");
            // get all CPUs
            var cpuCount = CpuID.GetPhysicalProcessorCount();

            Helpers.ConsolePrint(Tag,
                CpuID.IsHypeThreadingEnabled()
                    ? "HyperThreadingEnabled = TRUE"
                    : "HyperThreadingEnabled = FALSE");

            // get all cores (including virtual - HT can benefit mining)
            var threadsPerCpu = CpuID.GetVirtualCoresCount() / cpuCount;

            failed64Bit = !Helpers.Is64BitOperatingSystem;
            failedCpuCount = threadsPerCpu * cpuCount > 64;

            // TODO important move this to settings
            var threadsPerCpuMask = threadsPerCpu;
            Globals.ThreadsPerCpu = threadsPerCpu;

            var cpus = new List<CpuComputeDevice>();

            if (CpuUtils.IsCpuMiningCapable() && !failed64Bit && !failedCpuCount)
            {
                if (cpuCount == 1)
                {
                    cpus.Add(new CpuComputeDevice(0, "CPU0", CpuID.GetCpuName().Trim(), threadsPerCpu, 0, 1));
                }
                else if (cpuCount > 1)
                {
                    for (var i = 0; i < cpuCount; i++)
                    {
                        cpus.Add(
                            new CpuComputeDevice(i, "CPU" + i, CpuID.GetCpuName().Trim(), threadsPerCpu,
                                CpuID.CreateAffinityMask(i, threadsPerCpuMask), i + 1)
                        );
                    }
                }
            }

            Helpers.ConsolePrint(Tag, "QueryCpus END");

            return cpus;
        }
    }
}
