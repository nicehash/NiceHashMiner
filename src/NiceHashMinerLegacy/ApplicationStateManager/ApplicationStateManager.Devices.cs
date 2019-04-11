// TESTNET
#if TESTNET || TESTNETDEV
﻿using NiceHashMiner.Devices;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        #region device state checkers
        public static bool IsEnableAllDevicesRedundantOperation()
        {
            var allEnabled = ComputeDeviceManager.Available.Devices.All(dev => !dev.IsDisabled);
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
                devicesToDisable.AddRange(ComputeDeviceManager.Available.Devices.Where(dev => !dev.IsDisabled));
            }
            else
            {
                var devWithUUID = ComputeDeviceManager.Available.GetDeviceWithUuidOrB64Uuid(uuid);
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
#endif
