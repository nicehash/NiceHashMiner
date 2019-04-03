using NiceHashMiner.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices.Querying
{
    internal static class CpuQuery
    {
        private const string Tag = "QueryCPU";

        public static IEnumerable<CpuComputeDevice> QueryCpus()
        {
            Helpers.ConsolePrint(Tag, "QueryCpus START");
            // get all CPUs
            var cpuCount = CpuID.GetPhysicalProcessorCount();
            var cpuName = CpuID.GetCpuName().Trim();

            if (!CpuUtils.IsCpuMiningCapable())
            {
                // TODO LOG
                return Enumerable.Empty<CpuComputeDevice>();
            }

            Helpers.ConsolePrint(Tag,
                WindowsManagementObjectSearcher.IsHypeThreadingEnabled
                    ? "HyperThreadingEnabled = TRUE"
                    : "HyperThreadingEnabled = FALSE");

            // get all cores (including virtual - HT can benefit mining)
            var threadsPerCpu = WindowsManagementObjectSearcher.VirtualCoresCount / cpuCount;
            // TODO important move this to settings
            var threadsPerCpuMask = threadsPerCpu;
            //if (threadsPerCpu * cpuCount > 64)
            //{
            //    // set lower 
            //    threadsPerCpuMask = 64;
            //}

            var cpus = new List<CpuComputeDevice>();

            if (cpuCount == 1)
            {
                cpus.Add(new CpuComputeDevice(0, "CPU0", cpuName, threadsPerCpu, 0, 1));
            }
            else if (cpuCount > 1)
            {
                for (var i = 0; i < cpuCount; i++)
                {
                    var affinityMask = CpuUtils.CreateAffinityMask(i, threadsPerCpuMask);
                    cpus.Add(new CpuComputeDevice(i, "CPU" + i, cpuName, threadsPerCpu, affinityMask, i + 1)
                    );
                }
            }

            Helpers.ConsolePrint(Tag, "QueryCpus END");

            return cpus;
        }

        public static Task<IEnumerable<CpuComputeDevice>> QueryCpusAsync()
        {
            return Task.Run(() => QueryCpus());
        }
    }
}
