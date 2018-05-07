using NiceHashMiner.Enums;
using NVIDIA.NVAPI;

namespace NiceHashMiner.Devices
{
    internal class CudaComputeDevice : ComputeDevice
    {
        private readonly NvPhysicalGpuHandle _nvHandle; // For NVAPI
        private const int GpuCorePState = 0; // memcontroller = 1, videng = 2
        
        protected int SMMajor;
        protected int SMMinor;

        public override float Load
        {
            get
            {
                var load = -1;
                var pStates = new NvPStates
                {
                    Version = NVAPI.GPU_PSTATES_VER,
                    PStates = new NvPState[NVAPI.MAX_PSTATES_PER_GPU]
                };
                if (NVAPI.NvAPI_GPU_GetPStates != null)
                {
                    var result = NVAPI.NvAPI_GPU_GetPStates(_nvHandle, ref pStates);
                    if (result != NvStatus.OK)
                    {
                        Helpers.ConsolePrint("NVAPI", "Load get failed with status: " + result);
                    }
                    else if (pStates.PStates[GpuCorePState].Present)
                    {
                        load = pStates.PStates[GpuCorePState].Percentage;
                    }
                }
                return load;
            }
        }

        public override float Temp
        {
            get
            {
                var temp = -1f;
                if (NVAPI.NvAPI_GPU_GetThermalSettings != null)
                {
                    var settings = new NvGPUThermalSettings
                    {
                        Version = NVAPI.GPU_THERMAL_SETTINGS_VER,
                        Count = NVAPI.MAX_THERMAL_SENSORS_PER_GPU,
                        Sensor = new NvSensor[NVAPI.MAX_THERMAL_SENSORS_PER_GPU]
                    };
                    var result = NVAPI.NvAPI_GPU_GetThermalSettings(_nvHandle, (int) NvThermalTarget.ALL, ref settings);
                    if (result != NvStatus.OK)
                    {
                        Helpers.ConsolePrint("NVAPI", "Temp get failed with status: " + result);
                    }
                    else
                    {
                        foreach (var sensor in settings.Sensor)
                        {
                            if (sensor.Target == NvThermalTarget.GPU)
                            {
                                temp = sensor.CurrentTemp;
                                break;
                            }
                        }
                    }
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

        public CudaComputeDevice(CudaDevice cudaDevice, DeviceGroupType group, int gpuCount,
            NvPhysicalGpuHandle nvHandle)
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
            Index = ID + ComputeDeviceManager.Avaliable.AvailCpus; // increment by CPU count

            _nvHandle = nvHandle;
        }
    }
}
