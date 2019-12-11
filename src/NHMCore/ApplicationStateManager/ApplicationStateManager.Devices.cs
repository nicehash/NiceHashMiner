using NHMCore.Mining;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {

        internal static async Task SetDeviceEnabledState(ComputeDevice dev, bool enabled)
        {
            if (!enabled)
            {
                await StopDeviceTask(dev, false);
            }
            dev.Enabled = enabled;
            Configs.ConfigManager.CommitBenchmarksForDevice(dev);
        }

        public static async Task SetDeviceEnabledState(object sender, (string uuid, bool enabled) args)
        {
            var (uuid, enabled) = args;
            // TODO log sender
            var devicesToSet = new List<ComputeDevice>();
            var isAllDevices = "*" == uuid;
            if (isAllDevices)
            {
                devicesToSet.AddRange(AvailableDevices.Devices);
            }
            else
            {
                var devWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(uuid);
                if (devWithUUID != null)
                {
                    devicesToSet.Add(devWithUUID);
                }
            }
            var tasks = devicesToSet.Select(dev => SetDeviceEnabledState(dev, enabled));
            // await tasks
            await Task.WhenAll(tasks);
            await UpdateDevicesToMineTask();
        }
    }
}
