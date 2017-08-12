using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;
using ATI.ADL;

namespace NiceHashMiner.Devices
{
    class AmdComputeDevice : ComputeDevice
    {
        int adapterIndex;  // For ADL

        public override uint FanSpeed {
            get {
                var adlf = new ADLFanSpeedValue();
                adlf.SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_RPM;
                var result = ADL.ADL_Overdrive5_FanSpeed_Get(adapterIndex, 0, ref adlf);
                if (result != ADL.ADL_SUCCESS) {
                    Helpers.ConsolePrint("ADL", "ADL fan getting failed with error code " + result);
                }
                return (uint)adlf.FanSpeed;
            }
        }

        public override float Temp {
            get {
                var adlt = new ADLTemperature();
                var result = ADL.ADL_Overdrive5_Temperature_Get(adapterIndex, 0, ref adlt);
                if (result != ADL.ADL_SUCCESS) {
                    Helpers.ConsolePrint("ADL", "ADL temp getting failed with error code " + result);
                }
                return adlt.Temperature * 0.001f;
            }
        }

        public override float Load {
            get {
                var adlp = new ADLPMActivity();
                var result = ADL.ADL_Overdrive5_CurrentActivity_Get(adapterIndex, ref adlp);
                if (result != ADL.ADL_SUCCESS) {
                    Helpers.ConsolePrint("ADL", "ADL load getting failed with error code " + result);
                }
                return adlp.ActivityPercent;
            }
        }
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
            BusID = amdDevice.BusID;
            Codename = amdDevice.Codename;
            InfSection = amdDevice.InfSection;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            DriverDisableAlgos = amdDevice.DriverDisableAlgos;
            Index = ID + ComputeDeviceManager.Avaliable.AvailCPUs + ComputeDeviceManager.Avaliable.AvailNVGPUs;
            adapterIndex = amdDevice.AdapterIndex;
        }
    }
}
