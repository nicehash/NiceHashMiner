using NHMCore.Mining;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        // TODO split into single and multiple devices
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
            // execute enabling/disabling
            foreach (var dev in devicesToSet)
            {
                if (!enabled)
                {
                    // TODO here we might want to await them all instead of each individually 
                    await StopDevice(dev);
                }

                dev.Enabled = enabled;
            }
            Configs.ConfigManager.GeneralConfigFileCommit();

            // finally refresh state
            RefreshDeviceListView?.Invoke(null, null);
        }
    }
}
