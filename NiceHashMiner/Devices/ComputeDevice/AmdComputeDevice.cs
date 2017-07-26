using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;

namespace NiceHashMiner.Devices
{
    class AmdComputeDevice : ComputeDevice
    {
        public AmdComputeDevice(AmdGpuDevice amdDevice, int GPUCount, bool isDetectionFallback) 
            : base(amdDevice.DeviceID,
                  amdDevice.DeviceName,
                  true,
                  DeviceGroupType.AMD_OpenCL,
                  amdDevice.IsEtherumCapable(),
                  DeviceType.AMD,
                  String.Format(International.GetText("ComputeDevice_Short_Name_AMD_GPU"), GPUCount),
                  amdDevice.DeviceGlobalMemory) {
            if (isDetectionFallback) {
                UUID = GetUUID(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType);
            } else {
                UUID = amdDevice.UUID;
            }
            Codename = amdDevice.Codename;
            InfSection = amdDevice.InfSection;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            DriverDisableAlgos = amdDevice.DriverDisableAlgos;
        }
    }
}
