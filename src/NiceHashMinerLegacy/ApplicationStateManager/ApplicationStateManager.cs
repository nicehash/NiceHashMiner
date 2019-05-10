using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
#region Version
        public static string LocalVersion { get; private set; }
        public static string OnlineVersion { get; private set; }

        public static void OnVersionUpdate(string version)
        {
            // update version
            if (OnlineVersion != version)
            {
                OnlineVersion = version;
            }
            if (OnlineVersion == null)
            {
                return;
            }

            // check if the online version is greater than current
            var programVersion = new Version(Application.ProductVersion);
            var onlineVersion = new Version(OnlineVersion);
            var ret = programVersion.CompareTo(onlineVersion);

            // not sure why BetaAlphaPostfixString is being checked
            if (ret < 0 || (ret == 0 && BetaAlphaPostfixString != ""))
            {
                var displayNewVer = string.Format(Translations.Tr("IMPORTANT! New version v{0} has\r\nbeen released. Click here to download it."), version);
                // display new version
                // notify all components
                DisplayVersion?.Invoke(null, displayNewVer);
            }
        }

        public static void VisitNewVersionUrl()
        {
            // let's not throw anything if online version is missing just go to releases
            var url = Links.VisitReleasesUrl;
            if (OnlineVersion != null)
            {
                url = Links.VisitNewVersionReleaseUrl + OnlineVersion;
            }
            Process.Start(url);
        }
#endregion

#region BtcBalance and fiat balance

        public static double BtcBalance { get; private set; }

        private static (double fiatBalance, string fiatSymbol) getFiatFromBtcBalance(double btcBalance)
        {
            var usdAmount = (BtcBalance * ExchangeRateApi.GetUsdExchangeRate());
            var fiatBalance = ExchangeRateApi.ConvertToActiveCurrency(usdAmount);
            var fiatSymbol = ExchangeRateApi.ActiveDisplayCurrency;
            return (fiatBalance, fiatSymbol);
        }

        public static void OnBalanceUpdate(double btcBalance)
        {
            BtcBalance = btcBalance;
            // btc
            DisplayBTCBalance?.Invoke(null, BtcBalance);
            // fiat
            DisplayFiatBalance?.Invoke(null, getFiatFromBtcBalance(btcBalance));
        }
#endregion

        [Flags]
        public enum CredentialsValidState : uint
        {
            VALID,
            INVALID_BTC,
            INVALID_WORKER,
            INVALID_BTC_AND_WORKER // composed state
        }

        public static CredentialsValidState GetCredentialsValidState()
        {
            // assume it is valid
            var ret = CredentialsValidState.VALID;

            if (!BitcoinAddress.ValidateBitcoinAddress(ConfigManager.GeneralConfig.BitcoinAddress))
            {
                ret |= CredentialsValidState.INVALID_BTC;
            }
            if (!BitcoinAddress.ValidateWorkerName(ConfigManager.GeneralConfig.WorkerName))
            {
                ret |= CredentialsValidState.INVALID_WORKER;
            }

            return ret;
        }

        // TODO this function is probably not at the right place now
        // We call this when we change BTC and Workername and this is most likely wrong
        public static void ResetNiceHashStatsCredentials()
        {
            // check if we have valid credentials
            var state = GetCredentialsValidState();
            if (state == CredentialsValidState.VALID)
            {
                // Reset credentials
                var (btc, worker, group) = ConfigManager.GeneralConfig.GetCredentials();
                // TESTNET
#if TESTNET || TESTNETDEV
                NiceHashStats.SetCredentials(btc, worker, group);
#else
                // PRODUCTION
                NiceHashStats.SetCredentials(btc, worker);
#endif
            }
            else
            {
                // TODO notify invalid credentials?? send state?
            }
        }

        public enum SetResult
        {
            INVALID = 0,
            NOTHING_TO_CHANGE,
            CHANGED
        }

#region ServiceLocation

        public static string GetSelectedServiceLocationLocationUrl(AlgorithmType algorithmType, NhmConectionType conectionType)
        {
            // TODO make sure the ServiceLocation index is always valid
            var location = StratumService.SelectedServiceLocation;
            return StratumServiceHelpers.GetLocationUrl(algorithmType, location, conectionType);
        }

        public static SetResult SetServiceLocationIfValidOrDifferent(int serviceLocation)
        {
            if (serviceLocation == ConfigManager.GeneralConfig.ServiceLocation)
            {
                return SetResult.NOTHING_TO_CHANGE;
            }
            if (serviceLocation >= 0 && serviceLocation < StratumService.MiningLocations.Count)
            {
                SetServiceLocation(serviceLocation);
                return SetResult.CHANGED;
            }
            // invalid service location will default to first valid one - 0
            SetServiceLocation(0);
            return SetResult.INVALID;
        }

        private static void SetServiceLocation(int serviceLocation)
        {
            // change in memory and save changes to file
            ConfigManager.GeneralConfig.ServiceLocation = serviceLocation;
            ConfigManager.GeneralConfigFileCommit();
            // notify all components
            DisplayServiceLocation?.Invoke(null, serviceLocation);
        }
#endregion

#region BTC setter

        // make sure to pass in trimmedBtc
        public static SetResult SetBTCIfValidOrDifferent(string btc, bool skipCredentialsSet = false)
        {
            if (btc == ConfigManager.GeneralConfig.BitcoinAddress)
            {
                return SetResult.NOTHING_TO_CHANGE;
            }
            if (!BitcoinAddress.ValidateBitcoinAddress(btc))
            {
                return SetResult.INVALID;
            }
            SetBTC(btc);
            if (!skipCredentialsSet)
            {
                ResetNiceHashStatsCredentials();
            }
            return SetResult.CHANGED;
        }

        private static void SetBTC(string btc)
        {
            // change in memory and save changes to file
            ConfigManager.GeneralConfig.BitcoinAddress = btc;
            ConfigManager.GeneralConfigFileCommit();
            if (IsCurrentlyMining)
            {
                MiningManager.RestartMiners();
            }

            // notify all components
            DisplayBTC?.Invoke(null, btc);
        }
#endregion

#region Worker setter

        // make sure to pass in trimmed workerName
        // skipCredentialsSet when calling from RPC, workaround so RPC will work
        public static SetResult SetWorkerIfValidOrDifferent(string workerName, bool skipCredentialsSet = false)
        {
            if (workerName == ConfigManager.GeneralConfig.WorkerName)
            {
                return SetResult.NOTHING_TO_CHANGE;
            }
            if (!BitcoinAddress.ValidateWorkerName(workerName))
            {
                return SetResult.INVALID;
            }
            SetWorker(workerName);
            if (!skipCredentialsSet)
            {
                ResetNiceHashStatsCredentials();
            }
            
            return SetResult.CHANGED;
        }

        private static void SetWorker(string workerName)
        {
            // change in memory and save changes to file
            ConfigManager.GeneralConfig.WorkerName = workerName;
            ConfigManager.GeneralConfigFileCommit();
            // if mining update the mining manager
            if (IsCurrentlyMining)
            {
                MiningManager.RestartMiners();
            }
            // notify all components
            DisplayWorkerName?.Invoke(null, workerName);
        }
#endregion

#region Group setter

        // make sure to pass in trimmed GroupName
        // skipCredentialsSet when calling from RPC, workaround so RPC will work
        public static SetResult SetGroupIfValidOrDifferent(string groupName, bool skipCredentialsSet = false)
        {
            if (groupName == ConfigManager.GeneralConfig.RigGroup)
            {
                return SetResult.NOTHING_TO_CHANGE;
            }
            // TODO group validator
            var groupValid = true; /*!BitcoinAddress.ValidateGroupName(GroupName)*/
            if (!groupValid)
            {
                return SetResult.INVALID;
            }
            SetGroup(groupName);
            if (!skipCredentialsSet)
            {
                ResetNiceHashStatsCredentials();
            }

            return SetResult.CHANGED;
        }

        private static void SetGroup(string groupName)
        {
            // change in memory and save changes to file
            ConfigManager.GeneralConfig.RigGroup = groupName;
            ConfigManager.GeneralConfigFileCommit();
            // notify all components
            DisplayGroup?.Invoke(null, groupName);
        }
#endregion

        public static void ToggleActiveInactiveDisplay()
        {
            var allDevs = AvailableDevices.Devices;
            var devicesNotActive = allDevs.All(dev => dev.State != DeviceState.Mining && dev.State != DeviceState.Benchmarking);
            if (devicesNotActive)
            {
                DisplayMiningStopped?.Invoke(null, null);
            }
            else
            {
                DisplayMiningStarted?.Invoke(null, null);
            }
        }

        public static bool AnyInMiningState()
        {
            var allDevs = AvailableDevices.Devices;
            return allDevs.Any(dev => dev.State == DeviceState.Mining);
        } 


        public static bool IsCurrentlyMining { get; private set; }
        // StartMining function should be called only if all mining requirements are met, btc or demo, valid workername, and sma data
        // don't call this function ever unless credentials are valid or if we will be using Demo mining
        // And if there are missing mining requirements
        private static bool StartMining()
        {
            if (IsCurrentlyMining)
            {
                return false;
            }
            IsCurrentlyMining = true;
            StartMinerStatsCheckTimer();
            StartComputeDevicesCheckTimer();
            StartPreventSleepTimer();
            StartInternetCheckTimer();
            DisplayMiningStarted?.Invoke(null, null);
            return true;
        }

        //public static bool StartDemoMining()
        //{
        //    StopMinerStatsCheckTimer();
        //    return false;
        //}

        private static bool StopMining(bool headless)
        {
            if (!IsCurrentlyMining)
            {
                return false;
            }
            MiningManager.StopAllMiners();

            PInvoke.PInvokeHelpers.AllowMonitorPowerdownAndSleep();
            IsCurrentlyMining = false;
            StopMinerStatsCheckTimer();
            StopComputeDevicesCheckTimer();
            StopPreventSleepTimer();
            StopInternetCheckTimer();
            DisplayMiningStopped?.Invoke(null, null);
            return true;
        }


        // TODO temporary here AfterDeviceQueryInitialization
        public static void AfterDeviceQueryInitialization()
        {
            ConfigManager.AfterDeviceQueryInitialization();
            RefreshDeviceListView?.Invoke(null, null);
            StartRefreshDeviceListViewTimer();
        }


        public static RigStatus CalcRigStatus()
        {
            if (!isInitFinished)
            {
                return RigStatus.Pending;
            }
            // TODO check if we are connected to ws if not retrun offline state

            // check devices
            var allDevs = AvailableDevices.Devices;
            // now assume we have all disabled
            var rigState = RigStatus.Disabled;
            // order matters, we are excluding pending state
            var anyDisabled = allDevs.Any(dev => dev.IsDisabled);
            if (anyDisabled) {
                rigState = RigStatus.Disabled;
            }
            var anyStopped = allDevs.Any(dev => dev.State == DeviceState.Stopped);
            if (anyStopped) {
                rigState = RigStatus.Stopped;
            }
            var anyMining = allDevs.Any(dev => dev.State == DeviceState.Mining);
            if (anyMining) {
                rigState = RigStatus.Mining;
            }
            var anyBenchmarking = allDevs.Any(dev => dev.State == DeviceState.Benchmarking);
            if (anyBenchmarking) {
                rigState = RigStatus.Benchmarking;
            }
            var anyError = allDevs.Any(dev => dev.State == DeviceState.Error);
            if (anyError) {
                rigState = RigStatus.Error;
            }           

            return rigState;
        }

        public static string CalcRigStatusString()
        {
            var rigState = CalcRigStatus();
            return rigState.ToString().ToUpper();
        }


        public enum CurrentFormState {
            Main,
            Benchmark,
            Settings,
        }
        public static CurrentFormState CurrentForm { get; set; } = CurrentFormState.Main;
        public static bool IsInBenchmarkForm() {
            return CurrentForm == CurrentFormState.Benchmark;
        }
        public static bool IsInSettingsForm() {
            return CurrentForm == CurrentFormState.Settings;
        }
    }
}
