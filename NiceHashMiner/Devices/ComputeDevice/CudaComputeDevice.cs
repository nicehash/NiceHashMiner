using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;
using NVIDIA.Nvml;
using NVIDIA.NVAPI;

namespace NiceHashMiner.Devices
{
    class CudaComputeDevice : ComputeDevice
    {
        nvmlDevice nvDevice;
        NvPhysicalGpuHandle nvHandle;
        private const int gpuCorePState = 0;  // memcontroller = 1, videng = 2

        public override float Load {
            get {/*
                var utilization = new nvmlUtilization();
                var result = NvmlNativeMethods.nvmlDeviceGetUtilizationRates(nvDevice, ref utilization);
                if (result != nvmlReturn.Success) {
                    printNVMLError(result);
                }
                return utilization.gpu;  // Will return 0 if no success*/
                int load = 0;
                var pStates = new NvPStates();
                pStates.Version = NVAPI.GPU_PSTATES_VER;
                pStates.PStates = new NvPState[NVAPI.MAX_PSTATES_PER_GPU];
                if (NVAPI.NvAPI_GPU_GetPStates != null) {
                    var result = NVAPI.NvAPI_GPU_GetPStates(nvHandle, ref pStates);
                    if (result != NvStatus.OK) {
                        Helpers.ConsolePrint("NVAPI", "Load get failed with status: " + result);
                    } else if (pStates.PStates[gpuCorePState].Present) {
                        load = pStates.PStates[gpuCorePState].Percentage;
                    }
                }
                return load;
            }
        }
        public override float Temp {
            get {
                uint temp = 0;
                /*
                var result = NvmlNativeMethods.nvmlDeviceGetTemperature(nvDevice, nvmlTemperatureSensors.Gpu, ref temp);
                if (result != nvmlReturn.Success) {
                    printNVMLError(result);
                }
                */
                if (NVAPI.NvAPI_GPU_GetThermalSettings != null) {
                    var settings = new NvGPUThermalSettings();
                    settings.Version = NVAPI.GPU_THERMAL_SETTINGS_VER;
                    settings.Count = NVAPI.MAX_THERMAL_SENSORS_PER_GPU;
                    settings.Sensor = new NvSensor[NVAPI.MAX_THERMAL_SENSORS_PER_GPU];
                    var result = NVAPI.NvAPI_GPU_GetThermalSettings(nvHandle, (int)NvThermalTarget.ALL, ref settings);
                    if (result != NvStatus.OK) {
                        Helpers.ConsolePrint("NVAPI", "Temp get failed with status: " + result);
                    } else {
                        foreach (var sensor in settings.Sensor) {
                            if (sensor.Target == NvThermalTarget.GPU) {
                                temp = sensor.CurrentTemp;
                                break;
                            }
                        }
                    }
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

        public CudaComputeDevice(CudaDevice cudaDevice, DeviceGroupType group, int GPUCount, NvPhysicalGpuHandle nvHandle)
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

            this.nvHandle = nvHandle;
        }

        private void printNVMLError(nvmlReturn error) {
            Helpers.ConsolePrint("NVML", NvmlNativeMethods.nvmlErrorString(error));
        }
    }
}
