// PRODUCTION
#if !(TESTNET || TESTNETDEV || PRODUCTION_NEW)
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;
using NiceHashMiner.Stats.Models;

namespace NiceHashMiner.Stats
{
    internal static partial class NiceHashStats
    {
        private const int DeviceUpdateInterval = 60 * 1000;

        // Event handlers for socket
        public static event EventHandler OnConnectionEstablished;
        
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

        private static void SocketOnOnDataReceived(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.IsText)
                {
                    NHM.Common.Logger.Info("SOCKET", "Received: " + e.Data);
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

                        case "markets":
                            HandleMarkets(e.Data);
                            break;
                        case "balance":
                            SetBalance(message.value.Value);
                            break;
                        case "versions":
                            SetVersion(message.legacy.Value);
                            break;
                        case "burn":
                            ApplicationStateManager.Burn(message.message.Value);
                            break;
                        case "exchange_rates":
                            SetExchangeRates(message.data.Value);
                            break;
                    }
                }
            } catch (Exception er)
            {
                NHM.Common.Logger.Error("SOCKET", er.ToString());
            }
        }

        private static void SocketOnOnConnectionEstablished(object sender, EventArgs e)
        {
            DeviceStatus_Tick(null); // Send device to populate rig stats

            OnConnectionEstablished?.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region Incoming socket calls

        private static void SetVersion(string version)
        {
            Version = version;
            ApplicationStateManager.OnVersionUpdate(version);
        }

        #endregion

        #region Outgoing socket calls

        public static void SetCredentials(string btc, string worker, string group = "UNUSED STUB ONLY TO BE SAME AS TESTNET")
        {
            var data = new NicehashCredentials
            {
                btc = btc,
                worker = worker
            };
            if (CredentialValidators.ValidateBitcoinAddress(data.btc) && CredentialValidators.ValidateWorkerName(worker))
            {
                var sendData = JsonConvert.SerializeObject(data);

                // Send as task since SetCredentials is called from UI threads
                Task.Factory.StartNew(() => _socket?.SendData(sendData));
            }
        }

        private static void DeviceStatus_Tick(object state)
        {
            var devices = AvailableDevices.Devices;
            var deviceList = new List<JArray>();
            var activeIDs = MiningManager.GetActiveMinersIndexes();
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
                catch (Exception e) {
                    NHM.Common.Logger.Error("SOCKET", e.ToString());
                }
            }
            var data = new DeviceStatusMessage
            {
                devices = deviceList
            };
            var sendData = JsonConvert.SerializeObject(data);
            // This function is run every minute and sends data every run which has two auxiliary effects
            // Keeps connection alive and attempts reconnection if internet was dropped
            _socket?.SendData(sendData);
        }

        #endregion


        public static void StateChanged()
        {
            // STUB FROM TESTNET
        }
    }
}
#endif
