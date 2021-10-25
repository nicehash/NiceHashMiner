using Microsoft.Win32;
using NHM.Common;
using System;
using System.Management;
using System.Security.Principal;

namespace NHMCore.Utils
{
    public class SteamWatcher : IDisposable
    {
        // thanks to https://github.com/Microbolt for sharing code to detect if steam game is running
        private WindowsIdentity _currentUser = WindowsIdentity.GetCurrent();

        private string _query;

        private ManagementEventWatcher _watcher;

        private bool _isDisposed = false;

        public EventHandler<bool> OnSteamGameStartedChanged;

        public SteamWatcher()
        {
            if (SteamRegistryValueExists())
            {
                _query = "SELECT * FROM RegistryValueChangeEvent " +
                        "WHERE Hive='HKEY_USERS' " +
                        "AND KeyPath='" + _currentUser.User.Value + @"\\SOFTWARE\\Valve\\Steam' AND ValueName='RunningAppID'";

                _watcher = new ManagementEventWatcher(_query);
                _watcher.EventArrived +=
                    new EventArrivedEventHandler(registryEventHandler);
                _watcher.Start();
            }
            else
            {
                Console.WriteLine("Steam not installed.  Not Running Watcher");
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _watcher?.Dispose();
                _currentUser?.Dispose();
            }
        }

        /// <summary>
        /// Make sure that the Steam registry key we are checking exists so that we don't query an key that doesn't exist
        /// </summary>
        /// <returns>true if key exists</returns>
        private bool SteamRegistryValueExists()
        {
            try
            {
                string keyName = @"HKEY_USERS\" + _currentUser.User.Value + @"\SOFTWARE\Valve\Steam";
                string valueName = "RunningAppID";
                var invalidKey = Registry.GetValue(keyName, valueName, null) == null;
                var validKey = !invalidKey;
                return validKey;
            }
            catch (Exception e)
            {
                Logger.Error("SteamWatcher", "Error at checking if steam is installed: " + e.Message);
                return false;
            }
        }

        private void registryEventHandler(object sender, EventArrivedEventArgs e)
        {
            OnSteamGameStartedChanged?.Invoke(this, IsSteamGameRunning());
        }

        public bool IsSteamGameRunning()
        {
            if (!SteamRegistryValueExists()) return false;
            string keyName = @"HKEY_USERS\" + _currentUser.User.Value + @"\SOFTWARE\Valve\Steam";
            string valueName = "RunningAppID";

            if (Registry.GetValue(keyName, valueName, null) is int runningAppID)
            {
                var isSteamRunning = runningAppID != 0;
                return isSteamRunning;
            }
            return false;
        }
    }
}
