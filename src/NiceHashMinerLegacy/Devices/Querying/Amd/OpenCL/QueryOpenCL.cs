using System;
using System.Text;
using Newtonsoft.Json;
using NiceHashMiner.PInvoke;

namespace NiceHashMiner.Devices.Querying.Amd.OpenCL
{
    internal class QueryOpenCL
    {
        private const string Tag = "QueryOpenCL";

        protected virtual string GetQueryString()
        {
            return DeviceDetection.GetOpenCLDevices();
        }

        public bool TryQueryOpenCLDevices(out OpenCLDeviceDetectionResult result)
        {
            Helpers.ConsolePrint(Tag, "QueryOpenCLDevices START");

            var queryOpenCLDevicesString = "";
            try
            {
                queryOpenCLDevicesString = GetQueryString();
                result = JsonConvert.DeserializeObject<OpenCLDeviceDetectionResult>(queryOpenCLDevicesString, Globals.JsonSettings);
            }
            catch (Exception ex)
            {
                // TODO print AMD detection string
                Helpers.ConsolePrint(Tag, "AMDOpenCLDeviceDetection threw Exception: " + ex.Message);
                result = null;
            }

            var success = false;

            if (result == null)
            {
                Helpers.ConsolePrint(Tag,
                    "AMDOpenCLDeviceDetection found no devices. AMDOpenCLDeviceDetection returned: " +
                    queryOpenCLDevicesString);
            }
            else
            {
                success = true;
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("AMDOpenCLDeviceDetection found devices success:");
                foreach (var oclElem in result.Platforms)
                {
                    stringBuilder.AppendLine($"\tFound devices for platform: {oclElem.PlatformName}");
                    foreach (var oclDev in oclElem.Devices)
                    {
                        stringBuilder.AppendLine("\t\tDevice:");
                        stringBuilder.AppendLine($"\t\t\tDevice ID {oclDev.DeviceID}");
                        stringBuilder.AppendLine($"\t\t\tDevice NAME {oclDev._CL_DEVICE_NAME}");
                        stringBuilder.AppendLine($"\t\t\tDevice TYPE {oclDev._CL_DEVICE_TYPE}");
                    }
                }
                Helpers.ConsolePrint(Tag, stringBuilder.ToString());
            }
            Helpers.ConsolePrint(Tag, "QueryOpenCLDevices END");

            return success;
        }
    }
}
