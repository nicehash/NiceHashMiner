using Newtonsoft.Json;
using NiceHashMiner.PInvoke;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NiceHashMiner.Devices.Querying.Nvidia
{
    internal class CudaQuery
    {
        private const string Tag = "CudaQuery";

        public bool TryQueryCudaDevices(out List<CudaDevice> cudaDevices)
        {
            try
            {
                var queryCudaDevicesString = GetCudaQueryString();
                var cudaQueryResult = JsonConvert.DeserializeObject<CudaDeviceDetectionResult>(queryCudaDevicesString,
                    Globals.JsonSettings);
                cudaDevices = cudaQueryResult.CudaDevices;

                if (cudaDevices != null && cudaDevices.Count != 0) {
                    var driverVer = cudaQueryResult.DriverVersion.Split('.').Select(s => int.Parse(s)).ToArray();
                    CUDADevice.INSTALLED_NVIDIA_DRIVERS = new Version(driverVer[0], driverVer[1]);
                    return true;
                }
                

                Helpers.ConsolePrint(Tag,
                    "CudaDevicesDetection found no devices. CudaDevicesDetection returned: " +
                    queryCudaDevicesString);

                return false;
            }
            catch (Exception ex)
            {
                // TODO
                Helpers.ConsolePrint(Tag, "CudaDevicesDetection threw Exception: " + ex.Message);
                cudaDevices = null;
                return false;
            }
        }

        protected virtual string GetCudaQueryString()
        {
            return DeviceDetection.GetCUDADevices();
        }
    }
}
