using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;
using System.Diagnostics;

namespace NiceHashMiner.Devices
{
    class CPUComputeDevice : ComputeDevice
    {
        PerformanceCounter cpuCounter;

        public override float Load {
            get {
                try {
                    if (cpuCounter != null) return cpuCounter.NextValue();
                } catch (Exception e) { Helpers.ConsolePrint("CPUDIAG", e.ToString()); }
                return 0;
            }
        }

        public CPUComputeDevice(int id, string group, string name, int threads, ulong affinityMask, int CPUCount)
            : base(id,
                  name,
                  true,
                  DeviceGroupType.CPU,
                  false,
                  DeviceType.CPU,
                  String.Format(International.GetText("ComputeDevice_Short_Name_CPU"), CPUCount),
                  0) {
            Threads = threads;
            AffinityMask = affinityMask;
            UUID = GetUUID(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType);
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            Index = ID;  // Don't increment for CPU

            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
        }
    }
}
