// TESTNET
#if TESTNET || TESTNETDEV
﻿using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        // GENERATED CODE by nhmlCodeGen tool START
        // lock for adding and removing IDataVisualizers
        static object objectLock = new object();

        #region IBalanceBTCDisplayer DisplayBTCBalance double
        static event EventHandler<double> DisplayBTCBalance;
        private static void subscribeIBalanceBTCDisplayer(IDataVisualizer s)
        {
            if (!(s is IBalanceBTCDisplayer sIBalanceBTCDisplayer)) return;
            DisplayBTCBalance += sIBalanceBTCDisplayer.DisplayBTCBalance;
            // emit on single shot
            EventHandler<double> singleShotEvent = sIBalanceBTCDisplayer.DisplayBTCBalance;
            singleShotEvent.Invoke(null, BtcBalance);
        }

        private static void unsubscribeIBalanceBTCDisplayer(IDataVisualizer s)
        {
            if (s is IBalanceBTCDisplayer sIBalanceBTCDisplayer) DisplayBTCBalance -= sIBalanceBTCDisplayer.DisplayBTCBalance;
        }
        #endregion IBalanceBTCDisplayer DisplayBTCBalance double

        #region IBalanceFiatDisplayer DisplayFiatBalance (double fiatBalance, string fiatCurrencySymbol)
        static event EventHandler<(double fiatBalance, string fiatCurrencySymbol)> DisplayFiatBalance;
        private static void subscribeIBalanceFiatDisplayer(IDataVisualizer s)
        {
            if (!(s is IBalanceFiatDisplayer sIBalanceFiatDisplayer)) return;
            DisplayFiatBalance += sIBalanceFiatDisplayer.DisplayFiatBalance;
            // emit on single shot
            EventHandler<(double fiatBalance, string fiatCurrencySymbol)> singleShotEvent = sIBalanceFiatDisplayer.DisplayFiatBalance;
            singleShotEvent.Invoke(null, getFiatFromBtcBalance(BtcBalance));
        }

        private static void unsubscribeIBalanceFiatDisplayer(IDataVisualizer s)
        {
            if (s is IBalanceFiatDisplayer sIBalanceFiatDisplayer) DisplayFiatBalance -= sIBalanceFiatDisplayer.DisplayFiatBalance;
        }
        #endregion IBalanceFiatDisplayer DisplayFiatBalance (double fiatBalance, string fiatCurrencySymbol)

        #region IBTCDisplayer DisplayBTC string
        static event EventHandler<string> DisplayBTC;
        private static void subscribeIBTCDisplayer(IDataVisualizer s)
        {
            if (!(s is IBTCDisplayer sIBTCDisplayer)) return;
            DisplayBTC += sIBTCDisplayer.DisplayBTC;
            // emit on single shot
            EventHandler<string> singleShotEvent = sIBTCDisplayer.DisplayBTC;
            singleShotEvent.Invoke(null, ConfigManager.GeneralConfig.BitcoinAddress);
        }

        private static void unsubscribeIBTCDisplayer(IDataVisualizer s)
        {
            if (s is IBTCDisplayer sIBTCDisplayer) DisplayBTC -= sIBTCDisplayer.DisplayBTC;
        }
        #endregion IBTCDisplayer DisplayBTC string

        #region IGlobalMiningRateDisplayer DisplayGlobalMiningRate double
        static event EventHandler<double> DisplayGlobalMiningRate;
        private static void subscribeIGlobalMiningRateDisplayer(IDataVisualizer s)
        {
            if (!(s is IGlobalMiningRateDisplayer sIGlobalMiningRateDisplayer)) return;
            DisplayGlobalMiningRate += sIGlobalMiningRateDisplayer.DisplayGlobalMiningRate;
        }

        private static void unsubscribeIGlobalMiningRateDisplayer(IDataVisualizer s)
        {
            if (s is IGlobalMiningRateDisplayer sIGlobalMiningRateDisplayer) DisplayGlobalMiningRate -= sIGlobalMiningRateDisplayer.DisplayGlobalMiningRate;
        }
        #endregion IGlobalMiningRateDisplayer DisplayGlobalMiningRate double

        #region IGroupDisplayer DisplayGroup string
        static event EventHandler<string> DisplayGroup;
        private static void subscribeIGroupDisplayer(IDataVisualizer s)
        {
            if (!(s is IGroupDisplayer sIGroupDisplayer)) return;
            DisplayGroup += sIGroupDisplayer.DisplayGroup;
        }

        private static void unsubscribeIGroupDisplayer(IDataVisualizer s)
        {
            if (s is IGroupDisplayer sIGroupDisplayer) DisplayGroup -= sIGroupDisplayer.DisplayGroup;
        }
        #endregion IGroupDisplayer DisplayGroup string

        #region IMiningNotProfitableDisplayer DisplayMiningNotProfitable EventArgs
        static event EventHandler<EventArgs> _DisplayMiningNotProfitable;
        private static void subscribeIMiningNotProfitableDisplayer(IDataVisualizer s)
        {
            if (!(s is IMiningNotProfitableDisplayer sIMiningNotProfitableDisplayer)) return;
            _DisplayMiningNotProfitable += sIMiningNotProfitableDisplayer.DisplayMiningNotProfitable;
        }

        private static void unsubscribeIMiningNotProfitableDisplayer(IDataVisualizer s)
        {
            if (s is IMiningNotProfitableDisplayer sIMiningNotProfitableDisplayer) _DisplayMiningNotProfitable -= sIMiningNotProfitableDisplayer.DisplayMiningNotProfitable;
        }
        #endregion IMiningNotProfitableDisplayer DisplayMiningNotProfitable EventArgs

        #region IMiningProfitableDisplayer DisplayMiningProfitable EventArgs
        static event EventHandler<EventArgs> _DisplayMiningProfitable;
        private static void subscribeIMiningProfitableDisplayer(IDataVisualizer s)
        {
            if (!(s is IMiningProfitableDisplayer sIMiningProfitableDisplayer)) return;
            _DisplayMiningProfitable += sIMiningProfitableDisplayer.DisplayMiningProfitable;
        }

        private static void unsubscribeIMiningProfitableDisplayer(IDataVisualizer s)
        {
            if (s is IMiningProfitableDisplayer sIMiningProfitableDisplayer) _DisplayMiningProfitable -= sIMiningProfitableDisplayer.DisplayMiningProfitable;
        }
        #endregion IMiningProfitableDisplayer DisplayMiningProfitable EventArgs

        #region INoInternetConnectionDisplayer DisplayNoInternetConnection EventArgs
        static event EventHandler<EventArgs> _DisplayNoInternetConnection;
        private static void subscribeINoInternetConnectionDisplayer(IDataVisualizer s)
        {
            if (!(s is INoInternetConnectionDisplayer sINoInternetConnectionDisplayer)) return;
            _DisplayNoInternetConnection += sINoInternetConnectionDisplayer.DisplayNoInternetConnection;
        }

        private static void unsubscribeINoInternetConnectionDisplayer(IDataVisualizer s)
        {
            if (s is INoInternetConnectionDisplayer sINoInternetConnectionDisplayer) _DisplayNoInternetConnection -= sINoInternetConnectionDisplayer.DisplayNoInternetConnection;
        }
        #endregion INoInternetConnectionDisplayer DisplayNoInternetConnection EventArgs

        #region IServiceLocationDisplayer DisplayServiceLocation int
        static event EventHandler<int> DisplayServiceLocation;
        private static void subscribeIServiceLocationDisplayer(IDataVisualizer s)
        {
            if (!(s is IServiceLocationDisplayer sIServiceLocationDisplayer)) return;
            DisplayServiceLocation += sIServiceLocationDisplayer.DisplayServiceLocation;
            // emit on single shot
            EventHandler<int> singleShotEvent = sIServiceLocationDisplayer.DisplayServiceLocation;
            singleShotEvent.Invoke(null, ConfigManager.GeneralConfig.ServiceLocation);
        }

        private static void unsubscribeIServiceLocationDisplayer(IDataVisualizer s)
        {
            if (s is IServiceLocationDisplayer sIServiceLocationDisplayer) DisplayServiceLocation -= sIServiceLocationDisplayer.DisplayServiceLocation;
        }
        #endregion IServiceLocationDisplayer DisplayServiceLocation int

        #region IStartMiningDisplayer DisplayMiningStarted EventArgs
        static event EventHandler<EventArgs> DisplayMiningStarted;
        private static void subscribeIStartMiningDisplayer(IDataVisualizer s)
        {
            if (!(s is IStartMiningDisplayer sIStartMiningDisplayer)) return;
            DisplayMiningStarted += sIStartMiningDisplayer.DisplayMiningStarted;
        }

        private static void unsubscribeIStartMiningDisplayer(IDataVisualizer s)
        {
            if (s is IStartMiningDisplayer sIStartMiningDisplayer) DisplayMiningStarted -= sIStartMiningDisplayer.DisplayMiningStarted;
        }
        #endregion IStartMiningDisplayer DisplayMiningStarted EventArgs

        #region IStopMiningDisplayer DisplayMiningStopped EventArgs
        static event EventHandler<EventArgs> DisplayMiningStopped;
        private static void subscribeIStopMiningDisplayer(IDataVisualizer s)
        {
            if (!(s is IStopMiningDisplayer sIStopMiningDisplayer)) return;
            DisplayMiningStopped += sIStopMiningDisplayer.DisplayMiningStopped;
        }

        private static void unsubscribeIStopMiningDisplayer(IDataVisualizer s)
        {
            if (s is IStopMiningDisplayer sIStopMiningDisplayer) DisplayMiningStopped -= sIStopMiningDisplayer.DisplayMiningStopped;
        }
        #endregion IStopMiningDisplayer DisplayMiningStopped EventArgs

        #region IVersionDisplayer DisplayVersion string
        static event EventHandler<string> DisplayVersion;
        private static void subscribeIVersionDisplayer(IDataVisualizer s)
        {
            if (!(s is IVersionDisplayer sIVersionDisplayer)) return;
            DisplayVersion += sIVersionDisplayer.DisplayVersion;
        }

        private static void unsubscribeIVersionDisplayer(IDataVisualizer s)
        {
            if (s is IVersionDisplayer sIVersionDisplayer) DisplayVersion -= sIVersionDisplayer.DisplayVersion;
        }
        #endregion IVersionDisplayer DisplayVersion string

        #region IWorkerNameDisplayer DisplayWorkerName string
        static event EventHandler<string> DisplayWorkerName;
        private static void subscribeIWorkerNameDisplayer(IDataVisualizer s)
        {
            if (!(s is IWorkerNameDisplayer sIWorkerNameDisplayer)) return;
            DisplayWorkerName += sIWorkerNameDisplayer.DisplayWorkerName;
            // emit on single shot
            EventHandler<string> singleShotEvent = sIWorkerNameDisplayer.DisplayWorkerName;
            singleShotEvent.Invoke(null, ConfigManager.GeneralConfig.WorkerName);
        }

        private static void unsubscribeIWorkerNameDisplayer(IDataVisualizer s)
        {
            if (s is IWorkerNameDisplayer sIWorkerNameDisplayer) DisplayWorkerName -= sIWorkerNameDisplayer.DisplayWorkerName;
        }
        #endregion IWorkerNameDisplayer DisplayWorkerName string

        #region IDevicesStateDisplayer RefreshDeviceListView EventArgs
        static event EventHandler<EventArgs> RefreshDeviceListView;
        private static void subscribeIDevicesStateDisplayer(IDataVisualizer s)
        {
            if (!(s is IDevicesStateDisplayer sIDevicesStateDisplayer)) return;
            RefreshDeviceListView += sIDevicesStateDisplayer.RefreshDeviceListView;
        }

        private static void unsubscribeIDevicesStateDisplayer(IDataVisualizer s)
        {
            if (s is IDevicesStateDisplayer sIDevicesStateDisplayer) RefreshDeviceListView -= sIDevicesStateDisplayer.RefreshDeviceListView;
        }
        #endregion IDevicesStateDisplayer RefreshDeviceListView EventArgs

        #region Subscribe/Unsubscribe 
        public static void SubscribeStateDisplayer(IDataVisualizer s)
        {
            lock (objectLock)
            {

                subscribeIBalanceBTCDisplayer(s);

                subscribeIBalanceFiatDisplayer(s);

                subscribeIBTCDisplayer(s);

                subscribeIGlobalMiningRateDisplayer(s);

                subscribeIGroupDisplayer(s);

                subscribeIMiningNotProfitableDisplayer(s);

                subscribeIMiningProfitableDisplayer(s);

                subscribeINoInternetConnectionDisplayer(s);

                subscribeIServiceLocationDisplayer(s);

                subscribeIStartMiningDisplayer(s);

                subscribeIStopMiningDisplayer(s);

                subscribeIVersionDisplayer(s);

                subscribeIWorkerNameDisplayer(s);

                subscribeIDevicesStateDisplayer(s);

            }
        }

        public static void UnsubscribeStateDisplayer(IDataVisualizer s)
        {
            lock (objectLock)
            {

                unsubscribeIBalanceBTCDisplayer(s);

                unsubscribeIBalanceFiatDisplayer(s);

                unsubscribeIBTCDisplayer(s);

                unsubscribeIGlobalMiningRateDisplayer(s);

                unsubscribeIGroupDisplayer(s);

                unsubscribeIMiningNotProfitableDisplayer(s);

                unsubscribeIMiningProfitableDisplayer(s);

                unsubscribeINoInternetConnectionDisplayer(s);

                unsubscribeIServiceLocationDisplayer(s);

                unsubscribeIStartMiningDisplayer(s);

                unsubscribeIStopMiningDisplayer(s);

                unsubscribeIVersionDisplayer(s);

                unsubscribeIWorkerNameDisplayer(s);

                unsubscribeIDevicesStateDisplayer(s);

            }
        }
        #endregion Subscribe/Unsubscribe 
        // GENERATED CODE by nhmlCodeGen tool END
    }
}
#endif
