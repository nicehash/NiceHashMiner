using ManagedCuda.Nvml;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using NVIDIA.NVAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices.Querying.Nvidia
{
    internal class NvidiaQuery : QueryGpu
    {
        private const string Tag = "NvidiaQuery";

        private static bool tryAddNvmlToEnvPathCalled = false;

        protected CudaQuery CudaQuery;

        public NvidiaQuery()
        {
            CudaQuery = new CudaQuery();
        }

        #region Query devices

        public List<CudaComputeDevice> QueryCudaDevices()
        {
            Logger.Info(Tag, "QueryCudaDevices START");

            var compDevs = new List<CudaComputeDevice>();

            if (!CudaQuery.TryQueryCudaDevices(out var cudaDevs))
            {
                Logger.Info(Tag, "QueryCudaDevices END");
                return compDevs;
            }
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("CudaDevicesDetection:");

            // Enumerate NVAPI handles and map to busid
            var idHandles = InitNvapi();

            tryAddNvmlToEnvPath();
            var nvmlInit = InitNvml();

            var numDevs = 0;

            foreach (var cudaDev in cudaDevs)
            {
                // We support SM3.0+
                var skip = cudaDev.SM_major < 3;
                var skipOrAdd = skip ? "SKIPED" : "ADDED";
                stringBuilder.AppendLine($"\t{skipOrAdd} device:");
                stringBuilder.AppendLine($"\t\tID: {cudaDev.DeviceID}");
                stringBuilder.AppendLine($"\t\tBusID: {cudaDev.pciBusID}");
                stringBuilder.AppendLine($"\t\tNAME: {cudaDev.GetName()}");
                stringBuilder.AppendLine($"\t\tVENDOR: {cudaDev.VendorName}");
                stringBuilder.AppendLine($"\t\tUUID: {cudaDev.UUID}");
                stringBuilder.AppendLine($"\t\tSM: {cudaDev.SM_major}.{cudaDev.SM_minor}");
                stringBuilder.AppendLine($"\t\tMEMORY: {cudaDev.DeviceGlobalMemory}");

                if (skip) continue;

                var nvmlHandle = new nvmlDevice();

                if (nvmlInit)
                {
                    var ret = NvmlNativeMethods.nvmlDeviceGetHandleByUUID(cudaDev.UUID, ref nvmlHandle);
                    stringBuilder.AppendLine(
                        "\t\tNVML HANDLE: " +
                        $"{(ret == nvmlReturn.Success ? nvmlHandle.Pointer.ToString() : $"Failed with code ret {ret}")}");
                }

                idHandles.TryGetValue(cudaDev.pciBusID, out var handle);
                compDevs.Add(new CudaComputeDevice(cudaDev, ++numDevs, handle, nvmlHandle));
            }

            Logger.Info(Tag, stringBuilder.ToString());
            Logger.Info(Tag, "QueryCudaDevices END");

            SortBusIDs(compDevs);

            return compDevs;
        }

        private static Dictionary<int, NvPhysicalGpuHandle> InitNvapi()
        {
            var idHandles = new Dictionary<int, NvPhysicalGpuHandle>();
            if (!NVAPI.IsAvailable)
            {
                return idHandles;
            }
            
            var handles = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
            if (NVAPI.NvAPI_EnumPhysicalGPUs == null)
            {
                Logger.Debug("NVAPI", "NvAPI_EnumPhysicalGPUs unavailable");
            }
            else
            {
                var status = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out _);
                if (status != NvStatus.OK)
                {
                    Logger.Debug("NVAPI", $"Enum physical GPUs failed with status: {status}");
                }
                else
                {
                    foreach (var handle in handles)
                    {
                        var idStatus = NVAPI.NvAPI_GPU_GetBusID(handle, out var id);

                        if (idStatus == NvStatus.EXPECTED_PHYSICAL_GPU_HANDLE) continue;

                        if (idStatus != NvStatus.OK)
                        {
                            Logger.Debug("NVAPI",
                                "Bus ID get failed with status: " + idStatus);
                        }
                        else
                        {
                            Logger.Debug("NVAPI", "Found handle for busid " + id);
                            idHandles[id] = handle;
                        }
                    }
                }
            }
            
            return idHandles;
        }

        private static bool InitNvml()
        {
            try
            {
                var ret = NvmlNativeMethods.nvmlInit();
                if (ret != nvmlReturn.Success)
                    throw new Exception($"NVML init failed with code {ret}");
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("NVML", e.Message);
                return false;
            }
        }

        private static void tryAddNvmlToEnvPath()
        {
            if (tryAddNvmlToEnvPathCalled) return;
            tryAddNvmlToEnvPathCalled = true; // call this ONLY ONCE AND NEVER AGAIN
            var nvmlRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                               "\\NVIDIA Corporation\\NVSMI";
            if (Directory.Exists(nvmlRootPath))
            {
                // Add to env so it can find nvml.dll
                var pathVar = Environment.GetEnvironmentVariable("PATH");
                pathVar += ";" + nvmlRootPath;
                Environment.SetEnvironmentVariable("PATH", pathVar);
            }
        }

        #endregion

        [Obsolete("Use WindowsManagementObjectSearcher.GetNvSmiDriver instead")]
        public static NvidiaSmiDriver GetNvSmiDriver()
        {            
            List<NvidiaSmiDriver> drivers = new List<NvidiaSmiDriver>();
            using (var searcher = new ManagementObjectSearcher(new WqlObjectQuery("SELECT DriverVersion FROM Win32_VideoController")).Get()) {
                try
                {
                    foreach (ManagementObject devicesInfo in searcher)
                    {
                        var winVerArray = devicesInfo.GetPropertyValue("DriverVersion").ToString().Split('.');
                        //we must parse windows driver format (ie. 25.21.14.1634) into nvidia driver format (ie. 416.34)
                        //nvidia format driver is inside last two elements of splited windows driver string (ie. 14 + 1634)
                        if (winVerArray.Length >= 2)
                        {
                            var firstPartOfVersion = winVerArray[winVerArray.Length - 2];
                            var secondPartOfVersion = winVerArray[winVerArray.Length - 1];
                            var shortVerArray = firstPartOfVersion + secondPartOfVersion;
                            var driverFull = shortVerArray.Remove(0, 1).Insert(3, ".").Split('.'); // we transform that string into "nvidia" version (ie. 416.83)
                            NvidiaSmiDriver driver = new NvidiaSmiDriver(Convert.ToInt32(driverFull[0]), Convert.ToInt32(driverFull[1])); //we create driver object from string version

                            if (drivers.Count == 0)
                                drivers.Add(driver);
                            else
                            {
                                foreach (var ver in drivers) //we are checking if there is other driver version on system
                                {
                                    if (ver.LeftPart != driver.LeftPart || ver.RightPart != driver.RightPart)
                                        drivers.Add(driver);
                                }
                            }
                            if (drivers.Count != 1)
                            {
                                //TODO what happens if there are more driver versions??!!
                            }
                        }
                    }
                    return drivers[0]; // TODO if we will support multiple drivers this must be changed
                }
                catch (Exception e) { }
            }
            return new NvidiaSmiDriver(-1, -1);
        }
    }

}
