using System;
using System.Diagnostics;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMiner.Utils.Guid;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Devices
{
    internal class CpuComputeDevice : ComputeDevice
    {
        private readonly PerformanceCounter _cpuCounter;

        public override float Load
        {
            get
            {
                try
                {
                    if (_cpuCounter != null) return _cpuCounter.NextValue();
                }
                catch (Exception e) { Helpers.ConsolePrint("CPUDIAG", e.ToString()); }
                return -1;
            }
        }

        public CpuComputeDevice(int id, string group, string name, int threads, ulong affinityMask, int cpuCount, CpuInfo info)
            : base(id,
                name,
                true,
                DeviceGroupType.CPU,
                false,
                DeviceType.CPU,
                string.Format(Translations.Tr("CPU#{0}"), cpuCount),
                0)
        {
            Threads = threads;
            AffinityMask = affinityMask;
            //Uuid = GetUuid(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType);
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            Index = ID; // Don't increment for CPU

            var hashedInfo = $"{id}--{info.VendorID}--{info.Family}--{info.Model}--{info.PhysicalID}--{info.ModelName}";
            var uuidHEX = UUID.V5(UUID.Nil().AsGuid(), hashedInfo).AsGuid().ToString();
            Uuid = $"CPU-{uuidHEX}";

            _cpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total"
            };
        }
    }
}
