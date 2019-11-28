using NHMCore.Interfaces.DataVisualizer;
using System;

namespace NHMCore
{
    static partial class ApplicationStateManager
    {
        // GENERATED CODE by nhmlCodeGen tool START
        // lock for adding and removing IDataVisualizers
        static object objectLock = new object();

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
                subscribeIGlobalMiningRateDisplayer(s);

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
                unsubscribeIGlobalMiningRateDisplayer(s);

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
