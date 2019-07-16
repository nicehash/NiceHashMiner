using NiceHashMiner.Devices;
using NiceHashMiner.Stats;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
#region device state checkers
        public static bool IsEnableAllDevicesRedundantOperation()
        {
            var allEnabled = AvailableDevices.Devices.All(dev => !dev.IsDisabled);
            return allEnabled;
        }

        public static bool IsDisableAllDevicesRedundantOperation()
        {
            return !IsEnableAllDevicesRedundantOperation();
        }

#endregion device state checkers

        public static void SetDeviceEnabledState(object sender, (string uuid, bool enabled) args)
        {
            var (uuid, enabled) = args;
            // TODO log sender
            var devicesToDisable = new List<ComputeDevice>();
            var isDisableAllDevices = "*" == uuid;
            if (isDisableAllDevices)
            {
                devicesToDisable.AddRange(AvailableDevices.Devices.Where(dev => !dev.IsDisabled));
            }
            else
            {
                var devWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(uuid);
                if (devWithUUID != null)
                {
                    devicesToDisable.Add(devWithUUID);
                }
            }
            // execute enabling/disabling
            foreach (var dev in devicesToDisable)
            {
                if (!enabled)
                {
                    StopDevice(dev);
                }
                dev.SetEnabled(enabled);
            }
            Configs.ConfigManager.GeneralConfigFileCommit();

            // finally refresh state
            RefreshDeviceListView?.Invoke(null, null);
            NiceHashStats.StateChanged();
        }
    }
}
