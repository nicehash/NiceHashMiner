using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NiceHashMiner.Configs;
using static NiceHashMiner.Translations;

namespace NiceHashMiner.Devices.Querying
{
    internal static class Cpu
    {
        private const string Tag = "QueryCPU";

        public static void QueryCpus()
        {
            Helpers.ConsolePrint(Tag, "QueryCpus START");
            // get all CPUs
            AvailableDevices.CpusCount = CpuID.GetPhysicalProcessorCount();
            AvailableDevices.IsHyperThreadingEnabled = CpuID.IsHypeThreadingEnabled();

            Helpers.ConsolePrint(Tag,
                AvailableDevices.IsHyperThreadingEnabled
                    ? "HyperThreadingEnabled = TRUE"
                    : "HyperThreadingEnabled = FALSE");

            // get all cores (including virtual - HT can benefit mining)
            var threadsPerCpu = CpuID.GetVirtualCoresCount() / AvailableDevices.CpusCount;

            if (!Helpers.Is64BitOperatingSystem)
            {
                if (ConfigManager.GeneralConfig.ShowDriverVersionWarning)
                {
                    MessageBox.Show(Tr("NiceHash Miner Legacy works only on 64-bit version of OS for CPU mining. CPU mining will be disabled."),
                        Tr("Warning!"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                AvailableDevices.CpusCount = 0;
            }

            if (threadsPerCpu * AvailableDevices.CpusCount > 64)
            {
                if (ConfigManager.GeneralConfig.ShowDriverVersionWarning)
                {
                    MessageBox.Show(Tr("NiceHash Miner Legacy does not support more than 64 virtual cores. CPU mining will be disabled."),
                       Tr("Warning!"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                AvailableDevices.CpusCount = 0;
            }

            // TODO important move this to settings
            var threadsPerCpuMask = threadsPerCpu;
            Globals.ThreadsPerCpu = threadsPerCpu;

            if (CpuUtils.IsCpuMiningCapable())
            {
                if (AvailableDevices.CpusCount == 1)
                {
                    AvailableDevices.AddDevice(
                        new CpuComputeDevice(0, "CPU0", CpuID.GetCpuName().Trim(), threadsPerCpu, 0,
                            1)
                    );
                }
                else if (AvailableDevices.CpusCount > 1)
                {
                    for (var i = 0; i < AvailableDevices.CpusCount; i++)
                    {
                        AvailableDevices.AddDevice(
                            new CpuComputeDevice(i, "CPU" + i, CpuID.GetCpuName().Trim(), threadsPerCpu,
                                CpuID.CreateAffinityMask(i, threadsPerCpuMask), i + 1)
                        );
                    }
                }
            }

            Helpers.ConsolePrint(Tag, "QueryCpus END");
        }
    }
}
