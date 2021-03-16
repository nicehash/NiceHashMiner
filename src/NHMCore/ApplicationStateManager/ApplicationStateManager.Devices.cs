using NHMCore.Mining;
using System.Linq;
using System.Threading.Tasks;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {

        internal static async Task SetDeviceEnabledState(ComputeDevice dev, bool enabled)
        {
            if (!enabled) await StopDeviceTask(dev);
            dev.Enabled = enabled;
            Configs.ConfigManager.CommitBenchmarksForDevice(dev);
        }

        public static async Task SetDeviceEnabledState(object sender, (string uuid, bool enabled) args)
        {
            var (uuid, enabled) = args;
            // TODO log sender
            var isAllDevices = "*" == uuid;
            var devicesToSet = isAllDevices ? AvailableDevices.Devices : new ComputeDevice[] { AvailableDevices.GetDeviceWithUuidOrB64Uuid(uuid) };

            var tasks = devicesToSet
                .Where(dev => dev != null)
                .Distinct()
                .Select(dev => SetDeviceEnabledState(dev, enabled))
                .ToArray();
            // await tasks
            await Task.WhenAll(tasks);
        }
    }
}
