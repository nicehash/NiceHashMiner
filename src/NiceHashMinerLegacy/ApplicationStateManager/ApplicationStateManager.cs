using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
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
        #region StateDisplayers boilerplate
        static List<IDataVisualizer> _stateDisplayers = new List<IDataVisualizer>();

        public static void SubscribeStateDisplayer(IDataVisualizer stateDisplayer)
        {
            _stateDisplayers.Add(stateDisplayer);
            //// Init added state displayer
            if (stateDisplayer is IBTCDisplayer sBtcDisplayer)
            {
                sBtcDisplayer.DisplayBTC(ConfigManager.GeneralConfig.BitcoinAddress);
            }
            if (stateDisplayer is IWorkerNameDisplayer sWorkerNameDisplayer)
            {
                sWorkerNameDisplayer.DisplayWorkerName(ConfigManager.GeneralConfig.WorkerName);
            }
            if (stateDisplayer is IServiceLocationDisplayer sServiceLocationDisplayer)
            {
                sServiceLocationDisplayer.DisplayServiceLocation(ConfigManager.GeneralConfig.ServiceLocation);
            }
            if (stateDisplayer is IBalanceBTCDisplayer sBalanceBTCDisplayer)
            {
                sBalanceBTCDisplayer.DisplayBTCBalance(0);
            }
            if (stateDisplayer is IBalanceFiatDisplayer sBalanceFiatDisplayer)
            {
                sBalanceFiatDisplayer.DisplayFiatBalance(0, ExchangeRateApi.ActiveDisplayCurrency);
            }
        }

        public static void UnsubscribeStateDisplayer(IDataVisualizer stateDisplayer)
        {
            var success = _stateDisplayers.Remove(stateDisplayer);
            if (!success)
            {
                // TODO maybe log but than again we don't care about this shouldn't cause any issues
            }
        }
        #endregion


        public static string Title
        {
            get
            {
                return " v" + Application.ProductVersion + BetaAlphaPostfixString;
            }
        }

        #region Version
        private const string BetaAlphaPostfixString = " - Alpha";
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
                foreach (var s in _stateDisplayers)
                {
                    if (s is IVersionDisplayer sVersionDisplayer)
                    {
                        sVersionDisplayer.DisplayVersion(displayNewVer);
                    }
                }
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

        #region Balance
        public static double Balance { get; private set; }
        public static void OnBalanceUpdate(double btcBalance)
        {
            Balance = btcBalance;
            var usdAmount = (Balance * ExchangeRateApi.GetUsdExchangeRate());
            var fiatBalance = ExchangeRateApi.ConvertToActiveCurrency(usdAmount);
            // btc
            foreach (var s in _stateDisplayers)
            {
                if (s is IBalanceBTCDisplayer sBalanceBTCDisplayer)
                {
                    sBalanceBTCDisplayer.DisplayBTCBalance(Balance);
                }
            }
            // fiat
            foreach (var s in _stateDisplayers)
            {
                if (s is IBalanceFiatDisplayer sBalanceFiatDisplayer)
                {
                    sBalanceFiatDisplayer.DisplayFiatBalance(fiatBalance, ExchangeRateApi.ActiveDisplayCurrency);
                }
            }
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
            var (btc, worker, _) = ConfigManager.GeneralConfig.GetCredentials();
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

        #region ServiceLocation

        public static string GetSelectedServiceLocationLocationUrl(AlgorithmType algorithmType, NhmConectionType conectionType)
        {
            // TODO make sure the ServiceLocation index is always valid
            var location = StratumService.MiningLocations[ConfigManager.GeneralConfig.ServiceLocation];
            return StratumService.GetLocationUrl(algorithmType, location, conectionType);
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
            foreach (var s in _stateDisplayers)
            {
                if (s is IServiceLocationDisplayer sServiceLocationDisplayer)
                {
                    sServiceLocationDisplayer.DisplayServiceLocation(serviceLocation);
                }
            }
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
                MinersManager.UpdateBTC(btc);
            }
            
            // notify all components
            foreach (var s in _stateDisplayers)
            {
                if (s is IBTCDisplayer sBtcDisplayer)
                {
                    sBtcDisplayer.DisplayBTC(btc);
                }
            }
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
                MinersManager.UpdateWorker(workerName);
            }
            // notify all components
            foreach (var s in _stateDisplayers)
            {
                if (s is IWorkerNameDisplayer sWorkerNameDisplayer)
                {
                    sWorkerNameDisplayer.DisplayWorkerName(workerName);
                }
            }
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
            foreach (var s in _stateDisplayers)
            {
                if (s is IGroupDisplayer sGroupDisplayer)
                {
                    sGroupDisplayer.DisplayGroup(groupName);
                }
            }
        }
        #endregion


        public static bool IsCurrentlyMining { get; private set; }
        // StartMining function should be called only if all mining requirements are met, btc or demo, valid workername, and sma data
        // don't call this function ever unless credentials are valid or if we will be using Demo mining
        // And if there are missing mining requirements
        public static bool StartMining()
        {
            IsCurrentlyMining = true;
            StartMinerStatsCheckTimer();
            return false;
        }

        //public static bool StartDemoMining()
        //{
        //    StopMinerStatsCheckTimer();
        //    return false;
        //}

        public static bool StopMining()
        {
            IsCurrentlyMining = false;
            StopMinerStatsCheckTimer();
            return false;
        }
    }
}
