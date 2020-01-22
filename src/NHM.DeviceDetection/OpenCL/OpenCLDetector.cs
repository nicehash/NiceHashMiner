using NHM.Common;
using NHM.DeviceDetection.OpenCL.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.OpenCL
{
    internal static class OpenCLDetector
    {
        private const string Tag = "OpenCLDetector";

        public static async Task<(string rawOutput, OpenCLDeviceDetectionResult parsed)> TryQueryOpenCLDevicesAsync()
        {
            Logger.Info(Tag, "TryQueryOpenCLDevicesAsync START");
            var result = await DeviceDetectionPrinter.GetDeviceDetectionResultAsync<OpenCLDeviceDetectionResult>("ocl -n", 60 * 1000);
            Logger.Info(Tag, "TryQueryOpenCLDevicesAsync END");
            return result;
        }

        // if we get multiple platforms copy the OpenCL.dll to nhml root
        public static async Task<(string rawOutput, OpenCLDeviceDetectionResult parsed)> TryQueryOpenCLDevicesAsyncFallback()
        {
            Logger.Info(Tag, "TryQueryOpenCLDevicesAsyncFallback START");
            try
            {
                Logger.Info(Tag, "TryQueryOpenCLDevicesAsyncFallback trying to copy OpenCL.dll");
                File.Copy(Paths.AppRootPath("OpenCL", "OpenCL.dll"), Paths.Root, true);
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"TryQueryOpenCLDevicesAsyncFallback trying to copy OpenCL.dll error: {e.Message}");
            }
            var result = await DeviceDetectionPrinter.GetDeviceDetectionResultAsync<OpenCLDeviceDetectionResult>("ocl -n", 60 * 1000);
            Logger.Info(Tag, "TryQueryOpenCLDevicesAsyncFallback END");
            return result;
        }
    }
}
