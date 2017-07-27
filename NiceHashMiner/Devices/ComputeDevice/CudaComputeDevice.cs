using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;
using NVIDIA.Nvml;

namespace NiceHashMiner.Devices
{
    class CudaComputeDevice : ComputeDevice
    {
        nvmlDevice nvDevice;

        public override float Load {
            get {
                var utilization = new nvmlUtilization();
                var result = NvmlNativeMethods.nvmlDeviceGetUtilizationRates(nvDevice, ref utilization);
                if (result != nvmlReturn.Success) {
                    printNVMLError(result);
                }
                return utilization.gpu;  // Will return 0 if no success
            }
        }
        public override float Temp {
            get {
                uint temp = 0;
                var result = NvmlNativeMethods.nvmlDeviceGetTemperature(nvDevice, nvmlTemperatureSensors.Gpu, ref temp);
                if (result != nvmlReturn.Success) {
                    printNVMLError(result);
                }
                return temp;
            }
        }
        public override uint FanSpeed {
            get {
                uint fanSpeed = 0;
                var result = NvmlNativeMethods.nvmlDeviceGetFanSpeed(nvDevice, ref fanSpeed);
                if (result != nvmlReturn.Success) {
                    Helpers.ConsolePrint("NVML", NvmlNativeMethods.nvmlErrorString(result));
                }
                return fanSpeed;
            }
        }

        public CudaComputeDevice(CudaDevice cudaDevice, DeviceGroupType group, int GPUCount)
            : base((int)cudaDevice.DeviceID, 
                  cudaDevice.GetName(),
                  true,
                  group,
                  cudaDevice.IsEtherumCapable(),
                  DeviceType.NVIDIA,
                  String.Format(International.GetText("ComputeDevice_Short_Name_NVIDIA_GPU"), GPUCount),
                  cudaDevice.DeviceGlobalMemory) { 
            _SM_major = cudaDevice.SM_major;
            _SM_minor = cudaDevice.SM_minor;
            UUID = cudaDevice.UUID;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            Index = ID + ComputeDeviceManager.Avaliable.AvailCPUs;  // increment by CPU count

            var result = NvmlNativeMethods.nvmlDeviceGetHandleByUUID(UUID, ref nvDevice);
            if (result != nvmlReturn.Success) {
                Helpers.ConsolePrint("NVML", NvmlNativeMethods.nvmlErrorString(result));
            }
        }

        private void printNVMLError(nvmlReturn error) {
            Helpers.ConsolePrint("NVML", NvmlNativeMethods.nvmlErrorString(error));
        }
    }
}
