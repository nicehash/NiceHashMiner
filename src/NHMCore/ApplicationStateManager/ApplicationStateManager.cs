using NHM.Common.Enums;
using NHM.CommonWin32;
using NHM.UUID;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Nhmws;
using NHMCore.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        public static string RigID() => UUID.GetDeviceB64UUID();
        public static DispatcherObject App { get; set; }
        // change this if TOS changes
        public static int CurrentTosVer => 5;

        #region Credentials methods
        // execute after 5seconds. Finish execution on last event after 5seconds
        private static DelayedSingleExecActionTask _resetNiceHashStatsCredentialsDelayed = new DelayedSingleExecActionTask
            (
            ResetNiceHashStatsCredentials,
            new TimeSpan(0, 0, 5)
            );

        public static event EventHandler<bool> OnNhmwsConnectionChanged;
        public static void SetNhmwsConnectionChanged(bool isConnected)
        {
            App.Dispatcher.Invoke(() =>
            {
                OnNhmwsConnectionChanged?.Invoke(null, isConnected);
            });
        }


        static void ResetNiceHashStatsCredentials()
        {
            if (CredentialsSettings.Instance.IsCredentialValid)
            {
                // Reset credentials
                var (btc, worker, group) = CredentialsSettings.Instance.GetCredentials();
                NHWebSocket.ResetCredentials(btc, worker, group);
            }
            else
            {
                // TODO notify invalid credentials?? send state?
                // login without user if credentials are invalid
                NHWebSocket.ResetCredentials();
            }
        }

        #region BTC setter

        // make sure to pass in trimmedBtc
        public static async Task<NhmwsSetResult> SetBTCIfValidOrDifferent(string btc, bool skipCredentialsSet = false)
        {
            if (btc == CredentialsSettings.Instance.BitcoinAddress && btc != "")
            {
                return NhmwsSetResult.NOTHING_TO_CHANGE;
            }
            if (!CredentialValidators.ValidateBitcoinAddress(btc))
            {
                // TODO if RPC set only if valid if local then just set it
                //CredentialsSettings.Instance.BitcoinAddress = btc;
                return NhmwsSetResult.INVALID;
            }
            await SetBTC(btc);
            if (!skipCredentialsSet)
            {
                _resetNiceHashStatsCredentialsDelayed.ExecuteDelayed(CancellationToken.None);
            }
            return NhmwsSetResult.CHANGED;
        }

        private static async Task SetBTC(string btc)
        {
            // change in memory and save changes to file
            CredentialsSettings.Instance.BitcoinAddress = btc;
            ConfigManager.GeneralConfigFileCommit();
            if (Helpers.IsElevated) NHMRegistry.Set_QM_MiningaddressFromRegistry(btc);
            await MiningManager.ChangeUsername(CreateUsername(btc, RigID()));
        }
        #endregion

        #region Worker setter

        // make sure to pass in trimmed workerName
        // skipCredentialsSet when calling from RPC, workaround so RPC will work
        public static NhmwsSetResult SetWorkerIfValidOrDifferent(string workerName, bool skipCredentialsSet = false)
        {
            if (workerName == CredentialsSettings.Instance.WorkerName)
            {
                return NhmwsSetResult.NOTHING_TO_CHANGE;
            }
            if (!CredentialValidators.ValidateWorkerName(workerName))
            {
                return NhmwsSetResult.INVALID;
            }
            SetWorker(workerName);
            if (!skipCredentialsSet)
            {
                _resetNiceHashStatsCredentialsDelayed.ExecuteDelayed(CancellationToken.None);
            }

            return NhmwsSetResult.CHANGED;
        }

        private static void SetWorker(string workerName)
        {
            // change in memory and save changes to file
            CredentialsSettings.Instance.WorkerName = workerName;
            ConfigManager.GeneralConfigFileCommit();
        }
        #endregion

        #region Group setter

        // make sure to pass in trimmed GroupName
        // skipCredentialsSet when calling from RPC, workaround so RPC will work
        public static NhmwsSetResult SetGroupIfValidOrDifferent(string groupName, bool skipCredentialsSet = false)
        {
            if (groupName == CredentialsSettings.Instance.RigGroup)
            {
                return NhmwsSetResult.NOTHING_TO_CHANGE;
            }
            // TODO group validator
            var groupValid = true; /*!BitcoinAddress.ValidateGroupName(GroupName)*/
            if (!groupValid)
            {
                return NhmwsSetResult.INVALID;
            }
            SetGroup(groupName);
            if (!skipCredentialsSet)
            {
                _resetNiceHashStatsCredentialsDelayed.ExecuteDelayed(CancellationToken.None);
            }

            return NhmwsSetResult.CHANGED;
        }

        private static void SetGroup(string groupName)
        {
            // change in memory and save changes to file
            CredentialsSettings.Instance.RigGroup = groupName;
            ConfigManager.GeneralConfigFileCommit();
        }
        #endregion

        #endregion Credentials methods

        // StartMining function should be called only if all mining requirements are met, btc or demo, valid workername, and sma data
        // don't call this function ever unless credentials are valid or if we will be using Demo mining
        // And if there are missing mining requirements
        internal static bool StartMining()
        {
            StartComputeDevicesCheckTimer();
            StartInternetCheckTimer();
            return true;
        }

        internal static void StopMining()
        {
            StopComputeDevicesCheckTimer();
            StopInternetCheckTimer();
            DisplayNoInternetConnection(false); // hide warning
            DisplayMiningProfitable(true); // hide warning
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
            // ORDER MATTERS!!!, we are excluding pending state
            var anyDisabled = allDevs.Any(dev => dev.IsDisabled);
            if (anyDisabled)
            {
                rigState = RigStatus.Disabled;
            }
            var anyStopped = allDevs.Any(dev => dev.State == DeviceState.Stopped);
            if (anyStopped)
            {
                rigState = RigStatus.Stopped;
            }
#if NHMWS4
            var anyMining = allDevs.Any(dev => dev.State == DeviceState.Mining || dev.State == DeviceState.Gaming);
#else
            var anyMining = allDevs.Any(dev => dev.State == DeviceState.Mining);
#endif
            if (anyMining)
            {
                rigState = RigStatus.Mining;
            }
            var anyBenchmarking = allDevs.Any(dev => dev.State == DeviceState.Benchmarking);
            if (anyBenchmarking)
            {
                rigState = RigStatus.Benchmarking;
            }
            var anyError = allDevs.Any(dev => dev.State == DeviceState.Error);
            if (anyError)
            {
                rigState = RigStatus.Error;
            }

            return rigState;
        }

        public static string CalcRigStatusString()
        {
            var rigState = CalcRigStatus();
            return rigState switch
            {
                RigStatus.Offline => "OFFLINE",
                RigStatus.Stopped => "STOPPED",
                RigStatus.Mining => "MINING",
                RigStatus.Benchmarking => "BENCHMARKING",
                RigStatus.Error => "ERROR",
                RigStatus.Pending => "PENDING",
                RigStatus.Disabled => "DISABLED",
                _ => "UNKNOWN",
            };
        }
    }
}
