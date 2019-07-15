using Newtonsoft.Json;
using NiceHashMiner.Stats.Models;
using NiceHashMiner.Switching;
using NiceHashMiner.Utils;
using System;
using System.Threading;
using System.Windows.Forms;
using WebSocketSharp;

namespace NiceHashMiner.Stats
{
    public class NiceHashSocket
    {
        private WebSocket _webSocket = null;
        public bool IsAlive => _webSocket.ReadyState == WebSocketState.Open;
        private bool _attemptingReconnect;
        private bool _endConnection = false;
        private bool _restartConnection = false;
        private bool _connectionAttempted;
        private bool _connectionEstablished;
        private readonly Random _random = new Random();
        private readonly string _address;
        private readonly LoginMessage _login = new LoginMessage
        {
            version = "NHML/" + Application.ProductVersion,
            // TESTNET
#if TESTNET || TESTNETDEV || PRODUCTION_NEW
            protocol = 3
#else
            protocol = 1
#endif
        };
        
        public event EventHandler OnConnectionEstablished;
        public event EventHandler<MessageEventArgs> OnDataReceived;
        public event EventHandler OnConnectionLost;

        public NiceHashSocket(string address)
        {
            _address = address;
        }

        public void StartConnection(string btc = null, string worker = null, string group = null)
        {
            NHSmaData.InitializeIfNeeded();
            _connectionAttempted = true;
            // TESTNET
#if TESTNET || TESTNETDEV || PRODUCTION_NEW
            _login.rig = ApplicationStateManager.RigID;

            if (btc != null) _login.btc = btc;
            if (worker != null) _login.worker = worker;
            if (group != null) _login.group = group;
#endif

            try
            {
                if (_webSocket == null)
                {
                    _webSocket = new WebSocket(_address, true);

                    _webSocket.OnOpen += Login;
                    _webSocket.OnMessage += ReceiveCallback;
                    _webSocket.OnError += ErrorCallback;
                    _webSocket.OnClose += CloseCallback;
                    _webSocket.Log.Level = LogLevel.Debug;
                    _webSocket.Log.Output = (data, s) => NHM.Common.Logger.Info("SOCKET", data.ToString());
                    _webSocket.EnableRedirection = true;
                }
                else
                {
                    NHM.Common.Logger.Info("SOCKET", $"Credentials change reconnecting nhmws");
                    _connectionEstablished = false;
                    _restartConnection = true;
                    _webSocket?.Close(CloseStatusCode.Normal, $"Credentials change reconnecting {ApplicationStateManager.Title}.");
                }
                NHM.Common.Logger.Info("SOCKET", "Connecting");
                _webSocket.Connect();
                NHM.Common.Logger.Info("SOCKET", "Connected");
                _connectionEstablished = true;
                _restartConnection = false;
            } catch (Exception e)
            {
                NHM.Common.Logger.Error("SOCKET", e.ToString());
            }
        }

        public void EndConnection()
        {
            _endConnection = true;
            // TODO client away
            //CloseStatusCode.Away
            _webSocket?.Close(CloseStatusCode.Normal, $"Exiting {ApplicationStateManager.Title}");
        }

        private void ReceiveCallback(object sender, MessageEventArgs e)
        {
            OnDataReceived?.Invoke(this, e);
        }

        private static void ErrorCallback(object sender, ErrorEventArgs e)
        {
            NHM.Common.Logger.Info("NiceHashSocket", $"Error occured: {e.Message}");
        }

        private void CloseCallback(object sender, CloseEventArgs e)
        {
            NHM.Common.Logger.Info("NiceHashSocket", $"Connection closed code {e.Code}: {e.Reason}");
            if(!_restartConnection)
            {
                AttemptReconnect();
            }
        }

        private void Login(object sender, EventArgs e)
        {
            try
            {
                var loginJson = JsonConvert.SerializeObject(_login);
                SendData(loginJson);

                OnConnectionEstablished?.Invoke(null, EventArgs.Empty);
            } catch (Exception er)
            {
                NHM.Common.Logger.Info("SOCKET", er.Message);
            }
        }

        // Don't call SendData on UI threads, since it will block the thread for a bit if a reconnect is needed
        public bool SendData(string data, bool recurs = false)
        {
            //TESTNET
#if TESTNET || TESTNETDEV || PRODUCTION_NEW
            // skip sending if no btc set send only login
            if (CredentialValidators.ValidateBitcoinAddress(_login.btc) == false && data.Contains("{\"method\":\"login\"") == false)
            {
                NHM.Common.Logger.Info("SOCKET", "Skipping SendData no BTC address");
                return false;
            }
#endif
            try
            {
                // Make sure connection is open
                if (_webSocket != null && IsAlive)
                {
                    NHM.Common.Logger.Info("SOCKET", $"Sending data: {data}");
                    _webSocket.Send(data);
                    return true;
                } else if (_webSocket != null)
                {
                    if (AttemptReconnect() && !recurs)
                    {
                        // Reconnect was successful, send data again (safety to prevent recursion overload)
                        SendData(data, true);
                    } else
                    {
                        NHM.Common.Logger.Info("SOCKET", "Socket connection unsuccessfull, will try again on next device update (1min)");
                    }
                } else
                {
                    if (!_connectionAttempted)
                    {
                        NHM.Common.Logger.Info("SOCKET", "Data sending attempted before socket initialization");
                    } else
                    {
                        NHM.Common.Logger.Info("SOCKET", "webSocket not created, retrying");
                        StartConnection();
                    }
                }
            } catch (Exception e)
            {
                NHM.Common.Logger.Info("NiceHashSocket", $"Error occured while sending data: {e.Message}");
            }
            return false;
        }

        private bool AttemptReconnect()
        {
            if (_attemptingReconnect || _endConnection)
            {
                return false;
            }
            if (IsAlive)
            {
                // no reconnect needed
                return true;
            }
            _attemptingReconnect = true;
            var sleep = _connectionEstablished ? 10 + _random.Next(0, 20) : 0;
            NHM.Common.Logger.Info("SOCKET", $"Attempting reconnect in {sleep} seconds");
            // More retries on first attempt
            var retries = _connectionEstablished ? 5 : 25;
            if (_connectionEstablished)
            {
                // Don't wait if no connection yet
                Thread.Sleep(sleep * 1000);
            } else
            {
                // Don't not wait again
                _connectionEstablished = true;
            }
            for (var i = 0; i < retries; i++)
            {
                try
                {
                    _webSocket.Connect();
                    Thread.Sleep(100);
                    if (IsAlive)
                    {
                        _attemptingReconnect = false;
                        return true;
                    }
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message == "A series of reconnecting has failed.")
                    {
                        // Need to recreate websocket
                        NHM.Common.Logger.Info("SOCKET", "Recreating socket");
                        _webSocket = null;
                        StartConnection();
                        break;
                    }
                }
                catch (Exception e)
                {
                    NHM.Common.Logger.Info("NiceHashSocket", $"Error while attempting reconnect: {e.Message}");
                }
                Thread.Sleep(1000);
            }
            _attemptingReconnect = false;
            OnConnectionLost?.Invoke(null, EventArgs.Empty);
            return false;
        }
    }
}

