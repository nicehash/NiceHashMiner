using NiceHashMiner.Configs;
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

#region IMiningProfitableDisplayer DisplayMiningProfitable EventArgs
        static event EventHandler<bool> _DisplayMiningProfitability;
        private static void subscribeIMiningProfitableDisplayer(IDataVisualizer s)
        {
            if (!(s is IMiningProfitabilityDisplayer sIMiningProfitableDisplayer)) return;
            _DisplayMiningProfitability += sIMiningProfitableDisplayer.DisplayMiningProfitable;
        }

        private static void unsubscribeIMiningProfitableDisplayer(IDataVisualizer s)
        {
            if (s is IMiningProfitabilityDisplayer sIMiningProfitableDisplayer) _DisplayMiningProfitability -= sIMiningProfitableDisplayer.DisplayMiningProfitable;
        }
#endregion IMiningProfitableDisplayer DisplayMiningProfitable EventArgs

#region INoInternetConnectionDisplayer DisplayNoInternetConnection EventArgs
        static event EventHandler<bool> _DisplayNoInternetConnection;
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

                subscribeIGlobalMiningRateDisplayer(s);

                subscribeIGroupDisplayer(s);

                subscribeIMiningProfitableDisplayer(s);

                subscribeINoInternetConnectionDisplayer(s);

                subscribeIVersionDisplayer(s);

                subscribeIDevicesStateDisplayer(s);

            }
        }

        public static void UnsubscribeStateDisplayer(IDataVisualizer s)
        {
            lock (objectLock)
            {

                unsubscribeIBalanceBTCDisplayer(s);

                unsubscribeIBalanceFiatDisplayer(s);

                unsubscribeIGlobalMiningRateDisplayer(s);

                unsubscribeIGroupDisplayer(s);

                unsubscribeIMiningProfitableDisplayer(s);

                unsubscribeINoInternetConnectionDisplayer(s);

                unsubscribeIVersionDisplayer(s);

                unsubscribeIDevicesStateDisplayer(s);

            }
        }
#endregion Subscribe/Unsubscribe 
        // GENERATED CODE by nhmlCodeGen tool END
    }
}
