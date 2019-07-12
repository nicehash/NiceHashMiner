using NHM.UUID;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using NHM.Common;
using NHM.Common.Enums;
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
        public static string RigID { get; } = UUID.GetDeviceB64UUID();

        // change this if TOS changes
        public static int CurrentTosVer => 4;

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
            Helpers.VisitUrlLink(url);
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

        public static void OnAutoScaleBTCValuesChange()
        {
            // btc
            DisplayBTCBalance?.Invoke(null, BtcBalance);
            // fiat
            DisplayFiatBalance?.Invoke(null, getFiatFromBtcBalance(BtcBalance));
        }

        public static void OnBalanceUpdate(double btcBalance)
        {
            BtcBalance = btcBalance;
            // btc
            DisplayBTCBalance?.Invoke(null, BtcBalance);
            // fiat
            DisplayFiatBalance?.Invoke(null, getFiatFromBtcBalance(BtcBalance));
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

            if (!CredentialValidators.ValidateBitcoinAddress(ConfigManager.GeneralConfig.BitcoinAddress))
            {
                ret |= CredentialsValidState.INVALID_BTC;
            }
            if (!CredentialValidators.ValidateWorkerName(ConfigManager.GeneralConfig.WorkerName))
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
                NiceHashStats.SetCredentials(btc, worker, group);
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

#region BTC setter

        // make sure to pass in trimmedBtc
        public static SetResult SetBTCIfValidOrDifferent(string btc, bool skipCredentialsSet = false)
        {
            if (btc == ConfigManager.GeneralConfig.BitcoinAddress && btc != "")
            {
                return SetResult.NOTHING_TO_CHANGE;
            }
            if (!CredentialValidators.ValidateBitcoinAddress(btc))
            {
                ConfigManager.GeneralConfig.BitcoinAddress = btc;
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
            RestartMinersIfMining();
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
            if (!CredentialValidators.ValidateWorkerName(workerName))
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
            RestartMinersIfMining();
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

        // StartMining function should be called only if all mining requirements are met, btc or demo, valid workername, and sma data
        // don't call this function ever unless credentials are valid or if we will be using Demo mining
        // And if there are missing mining requirements
        private static bool StartMining()
        {
            StartMinerStatsCheckTimer();
            StartComputeDevicesCheckTimer();
            StartPreventSleepTimer();
            StartInternetCheckTimer();
            return true;
        }

        //public static bool StartDemoMining()
        //{
        //    StopMinerStatsCheckTimer();
        //    return false;
        //}

        private static bool StopMining(bool headless)
        {
            MiningManager.StopAllMiners();

            PInvoke.PInvokeHelpers.AllowMonitorPowerdownAndSleep();
            StopMinerStatsCheckTimer();
            StopComputeDevicesCheckTimer();
            StopPreventSleepTimer();
            StopInternetCheckTimer();
            DisplayNoInternetConnection(false); // hide warning
            DisplayMiningProfitable(true); // hide warning
            return true;
        }


        // TODO this thing should be dropped when we have bindable properties
        public static void UpdateDevicesStatesAndStartDeviceRefreshTimer()
        {
            MiningState.Instance.CalculateDevicesStateChange();
            RefreshDeviceListView?.Invoke(null, null);
            StartRefreshDeviceListViewTimer();
        }


        public static RigStatus CalcRigStatus()
        {
            if (!isInitFinished)
            {
                return RigStatus.Pending;
            }
            if (IsInBenchmarkForm() || IsInSettingsForm() || IsInPluginsForm())
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
            switch (rigState)
            {
                case RigStatus.Offline: return "OFFLINE";
                case RigStatus.Stopped: return "STOPPED";
                case RigStatus.Mining: return "MINING";
                case RigStatus.Benchmarking: return "BENCHMARKING";
                case RigStatus.Error: return "ERROR";
                case RigStatus.Pending: return "PENDING";
                case RigStatus.Disabled: return "DISABLED";
            }
            return "UNKNOWN";
        }


        public enum CurrentFormState {
            Main,
            Benchmark,
            Settings,
            Plugins
        }
        private static CurrentFormState _currentForm = CurrentFormState.Main;
        public static CurrentFormState CurrentForm
        {
            get => _currentForm;
            set
            {
                _currentForm = value;
                NiceHashStats.StateChanged();
            }
        }
        public static bool IsInBenchmarkForm() {
            return CurrentForm == CurrentFormState.Benchmark;
        }
        public static bool IsInSettingsForm() {
            return CurrentForm == CurrentFormState.Settings;
        }
        public static bool IsInPluginsForm()
        {
            return CurrentForm == CurrentFormState.Plugins;
        }
    }
}
