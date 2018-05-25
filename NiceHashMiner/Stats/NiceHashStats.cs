using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMiner.Switching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using NiceHashMinerLegacy.Common.Enums;
using WebSocketSharp;

namespace NiceHashMiner.Stats
{
    public class SocketEventArgs : EventArgs
    {
        public readonly string Message;

        public SocketEventArgs(string message)
        {
            Message = message;
        }
    }

    internal static class NiceHashStats
    {
        #region JSON Models
#pragma warning disable 649, IDE1006
        private class NicehashCredentials
        {
            public string method = "credentials.set";
            public string btc;
            public string worker;
        }

        private class NicehashDeviceStatus
        {
            public string method = "devices.status";
            public List<JArray> devices;
        }
        public class ExchangeRateJson
        {
            public List<Dictionary<string, string>> exchanges { get; set; }
            public Dictionary<string, double> exchanges_fiat { get; set; }
        }
#pragma warning restore 649, IDE1006
        #endregion

        private const int DeviceUpdateLaunchDelay = 20 * 1000;
        private const int DeviceUpdateInterval = 60 * 1000;
        
        public static double Balance { get; private set; }
        public static string Version { get; private set; }
        public static bool IsAlive => _socket?.IsAlive ?? false;

        // Event handlers for socket
        public static event EventHandler OnBalanceUpdate;

        public static event EventHandler OnSmaUpdate;
        public static event EventHandler OnVersionUpdate;
        public static event EventHandler OnConnectionLost;
        public static event EventHandler OnConnectionEstablished;
        public static event EventHandler<SocketEventArgs> OnVersionBurn;
        public static event EventHandler OnExchangeUpdate;

        private static NiceHashSocket _socket;
        
        private static System.Threading.Timer _deviceUpdateTimer;

        public static void StartConnection(string address)
        {
            if (_socket == null)
            {
                _socket = new NiceHashSocket(address);
                _socket.OnConnectionEstablished += SocketOnOnConnectionEstablished;
                _socket.OnDataReceived += SocketOnOnDataReceived;
                _socket.OnConnectionLost += SocketOnOnConnectionLost;
            }
            _socket.StartConnection();
            _deviceUpdateTimer = new System.Threading.Timer(DeviceStatus_Tick, null, DeviceUpdateInterval, DeviceUpdateInterval);
        }

        #region Socket Callbacks

        private static void SocketOnOnConnectionLost(object sender, EventArgs eventArgs)
        {
            OnConnectionLost?.Invoke(sender, eventArgs);
        }

        private static void SocketOnOnDataReceived(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.IsText)
                {
                    Helpers.ConsolePrint("SOCKET", "Received: " + e.Data);
                    dynamic message = JsonConvert.DeserializeObject(e.Data);
                    switch (message.method.Value)
                    {
                        case "sma":
                            {
                                // Try in case stable is not sent, we still get updated paying rates
                                try
                                {
                                    var stable = JsonConvert.DeserializeObject(message.stable.Value);
                                    SetStableAlgorithms(stable);
                                } catch
                                { }
                                SetAlgorithmRates(message.data);
                                break;
                            }

                        case "balance":
                            SetBalance(message.value.Value);
                            break;
                        case "versions":
                            SetVersion(message.legacy.Value);
                            break;
                        case "burn":
                            OnVersionBurn?.Invoke(null, new SocketEventArgs(message.message.Value));
                            break;
                        case "exchange_rates":
                            SetExchangeRates(message.data.Value);
                            break;
                    }
                }
            } catch (Exception er)
            {
                Helpers.ConsolePrint("SOCKET", er.ToString());
            }
        }

        private static void SocketOnOnConnectionEstablished(object sender, EventArgs e)
        {
            DeviceStatus_Tick(null); // Send device to populate rig stats

            OnConnectionEstablished?.Invoke(null, EventArgs.Empty);
        }

        #endregion

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
                        var algoKey = (AlgorithmType) algo[0].Value<int>();
                        payingDict[algoKey] = algo[1].Value<double>();
                    }
                }

                NHSmaData.UpdateSmaPaying(payingDict);
                
                OnSmaUpdate?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetStableAlgorithms(JArray stable)
        {
            var stables = stable.Select(algo => (AlgorithmType) algo.Value<int>());
            NHSmaData.UpdateStableAlgorithms(stables);
        }

        private static void SetBalance(string balance)
        {
            try
            {
                if (double.TryParse(balance, NumberStyles.Float, CultureInfo.InvariantCulture, out var bal))
                {
                    Balance = bal;
                    OnBalanceUpdate?.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        private static void SetVersion(string version)
        {
            Version = version;
            OnVersionUpdate?.Invoke(null, EventArgs.Empty);
        }

        private static void SetExchangeRates(string data)
        {
            try
            {
                var exchange = JsonConvert.DeserializeObject<ExchangeRateJson>(data);
                if (exchange?.exchanges_fiat != null && exchange.exchanges != null)
                {
                    foreach (var exchangePair in exchange.exchanges)
                    {
                        if (exchangePair.TryGetValue("coin", out var coin) &&
                            coin == "BTC" &&
                            exchangePair.TryGetValue("USD", out var usd) &&
                            double.TryParse(usd, NumberStyles.Float, CultureInfo.InvariantCulture, out var usdD))
                        {
                            ExchangeRateApi.UsdBtcRate = usdD;
                            break;
                        }
                    }

                    ExchangeRateApi.UpdateExchangesFiat(exchange.exchanges_fiat);

                    OnExchangeUpdate?.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SOCKET", e.ToString());
            }
        }

        #endregion

        #region Outgoing socket calls

        public static void SetCredentials(string btc, string worker)
        {
            var data = new NicehashCredentials
            {
                btc = btc,
                worker = worker
            };
            if (BitcoinAddress.ValidateBitcoinAddress(data.btc) && BitcoinAddress.ValidateWorkerName(worker))
            {
                var sendData = JsonConvert.SerializeObject(data);

                // Send as task since SetCredentials is called from UI threads
                Task.Factory.StartNew(() => _socket?.SendData(sendData));
            }
        }

        private static void DeviceStatus_Tick(object state)
        {
            var devices = ComputeDeviceManager.Available.Devices;
            var deviceList = new List<JArray>();
            var activeIDs = MinersManager.GetActiveMinersIndexes();
            foreach (var device in devices)
            {
                try
                {
                    var array = new JArray
                    {
                        device.Index,
                        device.Name
                    };
                    var status = Convert.ToInt32(activeIDs.Contains(device.Index)) + ((int) device.DeviceType + 1) * 2;
                    array.Add(status);
                    array.Add((int) Math.Round(device.Load));
                    array.Add((int) Math.Round(device.Temp));
                    array.Add(device.FanSpeed);

                    deviceList.Add(array);
                }
                catch (Exception e) { Helpers.ConsolePrint("SOCKET", e.ToString()); }
            }
            var data = new NicehashDeviceStatus
            {
                devices = deviceList
            };
            var sendData = JsonConvert.SerializeObject(data);
            // This function is run every minute and sends data every run which has two auxiliary effects
            // Keeps connection alive and attempts reconnection if internet was dropped
            _socket?.SendData(sendData);
        }

        #endregion

        public static string GetNiceHashApiData(string url, string worker)
        {
            var responseFromServer = "";
            try
            {
                var activeMinersGroup = MinersManager.GetActiveMinersGroup();

                var wr = (HttpWebRequest) WebRequest.Create(url);
                wr.UserAgent = "NiceHashMiner/" + Application.ProductVersion;
                if (worker.Length > 64) worker = worker.Substring(0, 64);
                wr.Headers.Add("NiceHash-Worker-ID", worker);
                wr.Headers.Add("NHM-Active-Miners-Group", activeMinersGroup);
                wr.Timeout = 30 * 1000;
                var response = wr.GetResponse();
                var ss = response.GetResponseStream();
                if (ss != null)
                {
                    ss.ReadTimeout = 20 * 1000;
                    var reader = new StreamReader(ss);
                    responseFromServer = reader.ReadToEnd();
                    if (responseFromServer.Length == 0 || responseFromServer[0] != '{')
                        throw new Exception("Not JSON!");
                    reader.Close();
                }
                response.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", ex.Message);
                return null;
            }

            return responseFromServer;
        }
    }
}
