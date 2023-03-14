using NHMCore.Mining;
using System.Linq;
using System.Threading.Tasks;
using NHM.Common.Enums;
using NHMCore.ApplicationState;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {

        internal static async Task SetDeviceEnabledState(ComputeDevice dev, bool enabled)
        {
            if (!enabled) await StopDeviceTask(dev);
            dev.Enabled = enabled;

            var rigStatus = CalcRigStatus();
            if (enabled && (rigStatus == RigStatus.Mining || rigStatus == RigStatus.Benchmarking)) await StartDeviceTask(dev);

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

            var thisDevice = AvailableDevices.GetDeviceWithUuidOrB64Uuid(uuid);
            var isAnyOtherDeviceMining = AvailableDevices.Devices?.Where(d => d.Enabled)?
                .SelectMany(d => d.AlgorithmSettings)?
                .Where(a => a.ComputeDevice.B64Uuid != thisDevice.B64Uuid)?
                .Any(a => a.IsCurrentlyMining);
            var isThisDeviceMining = thisDevice.AlgorithmSettings?.Any(a => a.IsCurrentlyMining);
            if (isAnyOtherDeviceMining != null && isThisDeviceMining != null) //when mining and stop by toggle, this device mining is false...
            {
                if (enabled)
                {
                    if ((bool)!isThisDeviceMining && !(bool)isAnyOtherDeviceMining && MiningState.Instance.MiningStoppedByToggle)
                    {
                        MiningState.Instance.MiningStoppedByToggle = false;
                        _ = StartAllAvailableDevicesTask();
                    }
                }
                else
                {
                    if ((bool)isThisDeviceMining && !(bool)isAnyOtherDeviceMining) //todo check if this device not mining by now
                    {
                        MiningState.Instance.MiningStoppedByToggle = true;
                    }
                }
            }

            // await tasks
            await Task.WhenAll(tasks);
        }
    }
}
