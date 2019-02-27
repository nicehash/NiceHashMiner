using Newtonsoft.Json;
using NiceHashMiner.PInvoke;
using System;
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

                if (cudaDevices != null && cudaDevices.Count != 0) return true;

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
