using Newtonsoft.Json;
using NiceHashMiner.PInvoke;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common;

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
                

                Logger.Info(Tag, $"CudaDevicesDetection found no devices. CudaDevicesDetection returned: {queryCudaDevicesString}");
                Helpers.ConsolePrint(Tag,
                    "CudaDevicesDetection found no devices. CudaDevicesDetection returned: " +
                    queryCudaDevicesString);

                return false;
            }
            catch (Exception ex)
            {
                // TODO
                Logger.Error(Tag, $"CudaDevicesDetection threw Exception: {ex.Message}");
                Helpers.ConsolePrint(Tag, "CudaDevicesDetection threw Exception: " + ex.Message);
                cudaDevices = null;
                return false;
            }
        }

        //public async Task<CudaDeviceDetectionResult> TryQueryCudaDevicesAsync()
        //{
        //    var result = await DeviceDetectionPrinter.GetDeviceDetectionResultAsync<CudaDeviceDetectionResult>("cuda -", 30 * 1000);
        //    if (result == null) return null;

        //    try
        //    {
        //        var queryCudaDevicesString = GetCudaQueryString();
        //        var cudaQueryResult = JsonConvert.DeserializeObject<CudaDeviceDetectionResult>(queryCudaDevicesString,
        //            Globals.JsonSettings);
        //        cudaDevices = cudaQueryResult.CudaDevices;

        //        if (cudaDevices != null && cudaDevices.Count != 0) return true;

        //        Helpers.ConsolePrint(Tag,
        //            "CudaDevicesDetection found no devices. CudaDevicesDetection returned: " +
        //            queryCudaDevicesString);

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        // TODO
        //        Helpers.ConsolePrint(Tag, "CudaDevicesDetection threw Exception: " + ex.Message);
        //        cudaDevices = null;
        //        return false;
        //    }
        //}

        protected virtual string GetCudaQueryString()
        {
            return DeviceDetection.GetCUDADevices();
        }
    }
}
