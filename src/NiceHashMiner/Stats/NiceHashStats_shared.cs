//#define DEBUG_MARKETS
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Switching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHM.Common.Enums;
using WebSocketSharp;
using NiceHashMiner.Stats.Models;

namespace NiceHashMiner.Stats
{
    internal static partial class NiceHashStats
    {
        private const int DeviceUpdateLaunchDelay = 20 * 1000;
        public static string Version { get; private set; }
        public static bool IsAlive => _socket?.IsAlive ?? false;

        // Event handlers for socket
        public static event EventHandler OnSmaUpdate;
        public static event EventHandler OnConnectionLost;
        public static event EventHandler OnExchangeUpdate;

        private static NiceHashSocket _socket;

        #region Socket Callbacks
        public static void EndConnection()
        {
            _socket?.EndConnection();
        }

        private static void SocketOnOnConnectionLost(object sender, EventArgs eventArgs)
        {
            OnConnectionLost?.Invoke(sender, eventArgs);
        }

        #endregion Socket Callbacks

        #region Incoming socket calls

        private static void SetAlgorithmRates(JArray data)
        {
            try
            {
                var payingDict = new Dictionary<AlgorithmType, double>();
                if (data != null)
                {
                    foreach (var algo in data)
                    {
                        var algoKey = (AlgorithmType)algo[0].Value<int>();
                        payingDict[algoKey] = algo[1].Value<double>();
                    }
                }

                NHSmaData.UpdateSmaPaying(payingDict);

                OnSmaUpdate?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                NHM.Common.Logger.Error("SOCKET", $"SetAlgorithmRates error: {e.Message}");
            }
        }

        private static void SetStableAlgorithms(JArray stable)
        {
            var stables = stable.Select(algo => (AlgorithmType)algo.Value<int>());
            NHSmaData.UpdateStableAlgorithms(stables);
        }

        private static void SetBalance(string balance)
        {
            try
            {
                if (double.TryParse(balance, NumberStyles.Float, CultureInfo.InvariantCulture, out var bal))
                {
                    ApplicationStateManager.OnBalanceUpdate(bal);
                }
            }
            catch (Exception e)
            {
                NHM.Common.Logger.Error("SOCKET", $"SetBalance error: {e.Message}");
            }
        }

        private static void SetExchangeRates(string data)
        {
            try
            {
                var exchange = JsonConvert.DeserializeObject<ExchangeRateJson>(data);
                if (exchange?.exchanges_fiat == null || exchange.exchanges == null) return;
                foreach (var exchangePair in exchange.exchanges)
                {
                    if (!exchangePair.TryGetValue("coin", out var coin) || coin != "BTC" ||
                        !exchangePair.TryGetValue("USD", out var usd) ||
                        !double.TryParse(usd, NumberStyles.Float, CultureInfo.InvariantCulture, out var usdD))
                        continue;

                    ExchangeRateApi.UsdBtcRate = usdD;
                    break;
                }

                ExchangeRateApi.UpdateExchangesFiat(exchange.exchanges_fiat);

                OnExchangeUpdate?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                NHM.Common.Logger.Error("SOCKET", $"SetExchangeRates error: {e.Message}");
            }
        }

#if DEBUG_MARKETS
        private static bool debugEU = false;
        private static bool debugUSA = false;
        private static int index = 0;
        private static void changeDebugMarkets()
        {
            if (index == 0)
            {
                debugEU = true;
                debugUSA = true;
            }
            if (index == 1)
            {
                debugEU = false;
                debugUSA = true;
            }
            if (index == 2)
            {
                debugEU = true;
                debugUSA = false;
            }
            if (index == 3)
            {
                debugEU = false;
                debugUSA = false;
            }
            index = (index + 1) % 4;
        }
#endif

        private static void HandleMarkets(string data)
        {
            try
            {
                var markets = JsonConvert.DeserializeObject<MarketsMessage>(data);
                var hasEU = markets.data.Contains("EU");
                var hasUSA = markets.data.Contains("USA");
#if !DEBUG_MARKETS
                StratumService.SetEnabled(hasEU, hasUSA);
#else
                changeDebugMarkets();
                StratumService.SetEnabled(debugEU, debugUSA);
#endif
            }
            catch (Exception e)
            {
                NHM.Common.Logger.Error("SOCKET", $"HandleMarkets error: {e.Message}");
            }
        }

#endregion Incoming socket calls
    }
}
