using ManagedCuda.Nvml;
using NVIDIA.NVAPI;
using System;
using System.Diagnostics;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Configs.Data;

namespace NiceHashMiner.Devices
{
    internal class CudaComputeDevice : ComputeDevice
    {
        private readonly NvPhysicalGpuHandle _nvHandle; // For NVAPI
        private readonly nvmlDevice _nvmlDevice; // For NVML
        private const int GpuCorePState = 0; // memcontroller = 1, videng = 2
        
        protected int SMMajor;
        protected int SMMinor;

        public readonly bool ShouldRunEthlargement;

        private readonly uint _minPowerLimit;
        private readonly uint _defaultPowerLimit;
        private readonly uint _maxPowerLimit;

        public uint PowerTarget { get; private set; }
        public PowerLevel PowerLevel { get; private set; }

        public bool PowerLimitsEnabled { get; private set; }

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
                string.Format(Translations.Tr("GPU#{0}"), gpuCount),
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

            ShouldRunEthlargement = cudaDevice.DeviceName.Contains("1080") || cudaDevice.DeviceName.Contains("Titan Xp");

            try
            {
                var powerInfo = new NvGPUPowerInfo
                {
                    Version = NVAPI.GPU_POWER_INFO_VER,
                    Entries = new NvGPUPowerInfoEntry[4]
                };

                var ret = NVAPI.NvAPI_DLL_ClientPowerPoliciesGetInfo(nvHandle, ref powerInfo);
                if (ret != NvStatus.OK)
                    throw new Exception(ret.ToString());

                Debug.Assert(powerInfo.Entries.Length == 4);

                if (powerInfo.Entries[0].MinPower == 0 || powerInfo.Entries[0].MaxPower == 0)
                {
                    throw new Exception("Power control not available!");
                }

                _minPowerLimit = powerInfo.Entries[0].MinPower;
                _maxPowerLimit = powerInfo.Entries[0].MaxPower;
                _defaultPowerLimit = powerInfo.Entries[0].DefPower;

                PowerLimitsEnabled = true;
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("NVML", $"Getting power info failed with message \"{e.Message}\", disabling power setting");
                PowerLimitsEnabled = false;
            }
        }

        // Will probably move this power stuff to its own class when refactoring NV into a diff project

        // nvPercent in thousands of percent, e.g. 100000 for 100%
        private bool SetPowerTarget(uint nvPercent)
        {
            if (!PowerLimitsEnabled) return false;
            if (NVAPI.NvAPI_DLL_ClientPowerPoliciesSetStatus == null)
            {
                Helpers.ConsolePrint("NVAPI", "Missing power set delegate, disabling power");
                PowerLimitsEnabled = false;
                return false;
            }

            // Value of 0 corresponds to not touching anything
            if (nvPercent == uint.MinValue)
            {
                PowerTarget = nvPercent;
                return true;
            }

            // Check if given value is within bounds
            if (nvPercent < _minPowerLimit)
                throw new PowerOutOfRangeException(_minPowerLimit);
            if (nvPercent > _maxPowerLimit)
                throw new PowerOutOfRangeException(_maxPowerLimit);

            var status = new NvGPUPowerStatus
            {
                Flags = 1,
                Entries = new NvGPUPowerStatusEntry[4]
            };
            status.Entries[0].Power = nvPercent;  // percent * 1000
            status.Version = NVAPI.GPU_POWER_STATUS_VER;

            try
            {
                var ret = NVAPI.NvAPI_DLL_ClientPowerPoliciesSetStatus(_nvHandle, ref status);
                if (ret != NvStatus.OK)
                    throw new Exception($"NVAPI failed with return {ret}");
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("NVAPI", e.ToString());
                return false;
            }

            PowerTarget = nvPercent;

            return true;
        }

        public bool SetPowerTarget(PowerLevel level)
        {
            switch (level)
            {
                case PowerLevel.Low:
                    PowerLevel = level;
                    return SetPowerTarget(_minPowerLimit);
                case PowerLevel.Medium:
                    PowerLevel = level;
                    return SetPowerTarget((uint) Math.Round((_minPowerLimit + _defaultPowerLimit) / 2d));
                case PowerLevel.High:
                    PowerLevel = level;
                    return SetPowerTarget(_defaultPowerLimit);
            }

            return false;
        }

        // percent is in hundreds, e.g. 100%
        public bool SetPowerTarget(double percent)
        {
            return SetPowerTarget((uint) Math.Round(percent * 1000));
        }

        public override void SetFromComputeDeviceConfig(ComputeDeviceConfig config)
        {
            base.SetFromComputeDeviceConfig(config);

            if (config.PowerLevel != PowerLevel.Custom)  // Placeholder
            {
                SetPowerTarget(config.PowerLevel);
            }
            else
            {
                SetPowerTarget(config.PowerTarget);
            }
        }

        public override ComputeDeviceConfig GetComputeDeviceConfig()
        {
            var config =  base.GetComputeDeviceConfig();
            config.PowerTarget = PowerTarget;
            config.PowerLevel = PowerLevel;

            return config;
        }
    }
}
