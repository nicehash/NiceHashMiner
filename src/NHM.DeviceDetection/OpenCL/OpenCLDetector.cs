using NHM.DeviceDetection.OpenCL.Models;
using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
