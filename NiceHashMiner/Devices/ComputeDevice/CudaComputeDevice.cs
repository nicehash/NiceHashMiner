using ManagedCuda.Nvml;
using NVIDIA.NVAPI;
using System;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Devices
{
    internal class CudaComputeDevice : ComputeDevice
    {
        private readonly NvPhysicalGpuHandle _nvHandle; // For NVAPI
        private readonly nvmlDevice _nvmlDevice; // For NVML
        private const int GpuCorePState = 0; // memcontroller = 1, videng = 2
        
        protected int SMMajor;
        protected int SMMinor;

        public override float Load
        {
            get
            {
                var load = -1;

                try
                {
                    var rates = new nvmlUtilization();
                    var ret = NvmlNativeMethods.nvmlDeviceGetUtilizationRates(_nvmlDevice, ref rates);
                    if (ret != nvmlReturn.Success)
                        throw new Exception($"NVML get load failed with code: {ret}");

                    load = (int) rates.gpu;
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("NVML", e.ToString());
                }

                return load;
            }
        }

        public override float Temp
        {
            get
            {
                var temp = -1f;

                try
                {
                    var utemp = 0u;
                    var ret = NvmlNativeMethods.nvmlDeviceGetTemperature(_nvmlDevice, nvmlTemperatureSensors.Gpu,
                        ref utemp);
                    if (ret != nvmlReturn.Success)
                        throw new Exception($"NVML get temp failed with code: {ret}");

                    temp = utemp;
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("NVML", e.ToString());
                }

                return temp;
            }
        }

        public override int FanSpeed
        {
            get
            {
                var fanSpeed = -1;
                if (NVAPI.NvAPI_GPU_GetTachReading != null)
                {
                    var result = NVAPI.NvAPI_GPU_GetTachReading(_nvHandle, out fanSpeed);
                    if (result != NvStatus.OK && result != NvStatus.NOT_SUPPORTED)
                    {
                        // GPUs without fans are not uncommon, so don't treat as error and just return -1
                        Helpers.ConsolePrint("NVAPI", "Tach get failed with status: " + result);
                        return -1;
                    }
                }
                return fanSpeed;
            }
        }

        public override double PowerUsage
        {
            get
            {
                try
                {
                    var power = 0u;
                    var ret = NvmlNativeMethods.nvmlDeviceGetPowerUsage(_nvmlDevice, ref power);
                    if (ret != nvmlReturn.Success)
                        throw new Exception($"NVML power get failed with status: {ret}");

                    return power * 0.001;
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("NVML", e.ToString());
                }

                return -1;
            }
        }

        public CudaComputeDevice(CudaDevice cudaDevice, DeviceGroupType group, int gpuCount,
            NvPhysicalGpuHandle nvHandle, nvmlDevice nvmlHandle)
            : base((int) cudaDevice.DeviceID,
                cudaDevice.GetName(),
                true,
                group,
                cudaDevice.IsEtherumCapable(),
                DeviceType.NVIDIA,
                string.Format(International.GetText("ComputeDevice_Short_Name_NVIDIA_GPU"), gpuCount),
                cudaDevice.DeviceGlobalMemory)
        {
            BusID = cudaDevice.pciBusID;
            SMMajor = cudaDevice.SM_major;
            SMMinor = cudaDevice.SM_minor;
            Uuid = cudaDevice.UUID;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            Index = ID + ComputeDeviceManager.Available.AvailCpus; // increment by CPU count

            _nvHandle = nvHandle;
            _nvmlDevice = nvmlHandle;
        }
    }
}
