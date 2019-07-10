// TESTNET
#if TESTNET || TESTNETDEV || PRODUCTION_NEW
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NiceHashMiner.Configs;
using NiceHashMiner.Stats.Models;
using NHM.Common.Enums;
using WebSocketSharp;
// static imports
using static NiceHashMiner.Stats.StatusCodes;

namespace NiceHashMiner.Stats
{
    internal static partial class NiceHashStats
    {
        private const int DeviceUpdateInterval = 45 * 1000;

        public static string VersionLink { get; private set; }

        // Event handlers for socket
        public static event EventHandler<DeviceUpdateEventArgs> OnDeviceUpdate;
        
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
            _socket.StartConnection(ConfigManager.GeneralConfig.BitcoinAddress, ConfigManager.GeneralConfig.WorkerName, ConfigManager.GeneralConfig.RigGroup);
            _deviceUpdateTimer = new System.Threading.Timer(MinerStatus_Tick, null, DeviceUpdateInterval, DeviceUpdateInterval);
        }

#region Socket Callbacks

        private static void SocketOnOnDataReceived(object sender, MessageEventArgs e)
        {
            ExecutedInfo info = null;
            var executed = false;
            int? id = null;
            try
            {
                if (e.IsText)
                {
                    info = ProcessData(e.Data, out executed, out id);
                }

                if (executed)
                {
                    SendExecuted(info, id);
                }
            }
            catch (RpcException rEr)
            {
                NHM.Common.Logger.Error("SOCKET", rEr.Message);
                if (!executed) return;
                NHM.Common.Logger.Error("SOCKET", $"Sending executed response with code {rEr.Code}");
                SendExecuted(info, id, rEr.Code, rEr.Message);
            }
            catch (Exception er)
            {
                NHM.Common.Logger.Error("SOCKET", er.Message);
            }
        }

        internal static ExecutedInfo ProcessData(string data, out bool executed, out int? id)
        {
            NHM.Common.Logger.Info("SOCKET", $"Received: {data}");
            dynamic message = JsonConvert.DeserializeObject(data);
            executed = false;

            if (message == null)
                throw new RpcException("No message found", ErrorCode.UnableToHandleRpc);

            id = (int?) message.id;
            switch (message.method.Value)
            {
                case "sma":
                {
                    // Try in case stable is not sent, we still get updated paying rates
                    try
                    {
                        var stable = JsonConvert.DeserializeObject(message.stable.Value);
                        SetStableAlgorithms(stable);
                    }
                    catch
                    { }
                    SetAlgorithmRates(message.data);
                    return null;
                }

                case "markets":
                    HandleMarkets(data);
                    break;
                case "balance":
                    SetBalance(message.value.Value);
                    return null;
                case "burn":
                    ApplicationStateManager.Burn(message.message.Value);
                    return null;
                case "exchange_rates":
                    SetExchangeRates(message.data.Value);
                    return null;
                case "essentials":
                    var ess = JsonConvert.DeserializeObject<EssentialsCall>(data);
                    ProcessEssentials(ess);
                    return null;
                case "mining.set.username":
                    executed = true;
                    throwIfWeCannotHanldeRPC();
                    var btc = (string)message.username;
                    return miningSetUsername(btc);
                case "mining.set.worker":
                    executed = true;
                    throwIfWeCannotHanldeRPC();
                    var worker = (string)message.worker;
                    return miningSetWorker(worker);
                case "mining.set.group":
                    executed = true;
                    throwIfWeCannotHanldeRPC();
                    var group = (string) message.group;
                    return miningSetGroup(group);
                case "mining.enable":
                    executed = true;
                    throwIfWeCannotHanldeRPC();
                    SetDevicesEnabled((string) message.device, true);
                    return null;
                case "mining.disable":
                    executed = true;
                    throwIfWeCannotHanldeRPC();
                    SetDevicesEnabled((string) message.device, false);
                    return null;
                case "mining.start":
                    executed = true;
                    throwIfWeCannotHanldeRPC();
                    StartMining((string) message.device);
                    return null;
                case "mining.stop":
                    executed = true;
                    throwIfWeCannotHanldeRPC();
                    StopMining((string) message.device);
                    return null;
                case "mining.set.power_mode":
                    executed = true;
                    throwIfWeCannotHanldeRPC();
                    SetPowerMode((string) message.device, (PowerLevel) message.power_mode);
                    return null;
            }
            
            throw new RpcException("Operation not supported", ErrorCode.UnableToHandleRpc);
        }

        private static bool isRpcMethod(string method) {
            // well pretty much all RPCs start with mining.*
            switch (method) {
                case "mining.set.username":
                case "mining.set.worker":
                case "mining.set.group":
                case "mining.enable":
                case "mining.disable":
                case "mining.start":
                case "mining.stop":
                case "mining.set.power_mode":
                    return true;
            }
            return false;
        }

        private static void throwIfWeCannotHanldeRPC() {
            var rigStatusPending = ApplicationStateManager.CalcRigStatus() == RigStatus.Pending;
            var formState = ApplicationStateManager.IsInBenchmarkForm() ? ". Rig is in benchmarks form" : "";
            if (ApplicationStateManager.IsInSettingsForm())
            {
                formState = ". Rig is in settings form";
            }
            if (ApplicationStateManager.IsInPluginsForm())
            {
                formState = ". Rig is in plugins form";
            }
            // throw if pending
            if (rigStatusPending)
            {
                throw new RpcException($"Cannot handle RPC call Rig is in PENDING state{formState}", ErrorCode.UnableToHandleRpc);
            }            
        }


        private static void SocketOnOnConnectionEstablished(object sender, EventArgs e)
        {
            // Send device to populate rig stats, and send device names
            SendMinerStatus(true);
        }

#endregion

#region Incoming socket calls

        private static void ProcessEssentials(EssentialsCall ess)
        {
            if (ess?.Versions?.Count > 1 && ess.Versions[1].Count == 2)
            {
                SetVersion(ess.Versions[1][0], ess.Versions[1][1]);
            }
        }

        private static void SetVersion(string version, string link)
        {
            Version = version;
            VersionLink = link;
            ApplicationStateManager.OnVersionUpdate(version);
        }

#region Credentials setters (btc/username, worker, group)
        private static ExecutedInfo miningSetUsername(string btc)
        {
            var userSetResult = ApplicationStateManager.SetBTCIfValidOrDifferent(btc, true);
            switch (userSetResult)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    throw new RpcException("Bitcoin address invalid", ErrorCode.InvalidUsername);
                case ApplicationStateManager.SetResult.CHANGED:
                    // we return executed
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    throw new RpcException($"Nothing to change btc \"{btc}\" already set", ErrorCode.RedundantRpc);
            }
            return new ExecutedInfo { NewBtc = btc };
        }

        private static ExecutedInfo miningSetWorker(string worker)
        {
            var workerSetResult = ApplicationStateManager.SetWorkerIfValidOrDifferent(worker, true);
            switch (workerSetResult)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    throw new RpcException("Worker name invalid", ErrorCode.InvalidWorker);
                case ApplicationStateManager.SetResult.CHANGED:
                    // we return executed
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    throw new RpcException($"Nothing to change worker name \"{worker}\" already set", ErrorCode.RedundantRpc);
            }
            return new ExecutedInfo { NewWorker = worker };
        }

        private static ExecutedInfo miningSetGroup(string group)
        {
            var groupSetResult = ApplicationStateManager.SetGroupIfValidOrDifferent(group, true);
            switch (groupSetResult)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    // TODO error code not correct
                    throw new RpcException("Group name invalid", ErrorCode.UnableToHandleRpc);
                case ApplicationStateManager.SetResult.CHANGED:
                    // we return executed
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    throw new RpcException($"Nothing to change group \"{group}\" already set", ErrorCode.RedundantRpc);
            }
            return new ExecutedInfo { NewRig = group };
        }
#endregion Credentials setters (btc/username, worker, group)

        private static bool SetDevicesEnabled(string devs, bool enabled)
        {
            bool allDevices = devs == "*";
            // get device with uuid if it exists, devs can be single device uuid
            var deviceWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(devs);

            // Check if RPC should execute
            // check if redundant rpc
            if (allDevices && enabled && ApplicationStateManager.IsEnableAllDevicesRedundantOperation()) {
                throw new RpcException("All devices are already enabled.", ErrorCode.RedundantRpc);
            }
            // all disable
            if (allDevices && !enabled && ApplicationStateManager.IsDisableAllDevicesRedundantOperation()) {
                throw new RpcException("All devices are already disabled.", ErrorCode.RedundantRpc);
            }
            // if single and doesn't exist
            if (!allDevices && deviceWithUUID == null) {
                throw new RpcException("Device not found", ErrorCode.NonExistentDevice);
            }
            // if we have the device but it is redundant
            if (!allDevices && deviceWithUUID.IsDisabled == !enabled) {
                var stateStr = enabled ? "enabled" : "disabled";
                throw new RpcException($"Devices with uuid {devs} is already {stateStr}.", ErrorCode.RedundantRpc);
            }

            // if got here than we can execute the call
            ApplicationStateManager.SetDeviceEnabledState(null, (devs, enabled));
            // TODO invoke the event for controls that use it
            OnDeviceUpdate?.Invoke(null, new DeviceUpdateEventArgs(AvailableDevices.Devices.ToList()));
            // TODO this used to return 'anyStillRunning' but we are actually checking if there are any still enabled left
            var anyStillEnabled = AvailableDevices.Devices.Any();
            return anyStillEnabled;
        }

#region Start
        private static void startMiningAllDevices() {
            var allDisabled = AvailableDevices.Devices.All(dev => dev.IsDisabled);
            if (allDisabled) {
                throw new RpcException("All devices are disabled cannot start", ErrorCode.DisabledDevice);
            }
            var (success, msg) = ApplicationStateManager.StartAllAvailableDevices(true);
            if (!success) {
                throw new RpcException(msg, ErrorCode.RedundantRpc);
            }
        }

        private static void startMiningOnDeviceWithUuid(string uuid) {
            string errMsgForUuid = $"Cannot start device with uuid {uuid}";
            // get device with uuid if it exists, devs can be single device uuid
            var deviceWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(uuid);
            if (deviceWithUUID == null) {
                throw new RpcException($"{errMsgForUuid}. Device not found.", ErrorCode.NonExistentDevice);
            }
            if (deviceWithUUID.IsDisabled) {
                throw new RpcException($"{errMsgForUuid}. Device is disabled.", ErrorCode.DisabledDevice);
            }
            var (success, msg) = ApplicationStateManager.StartDevice(deviceWithUUID);
            if (!success) {
                // TODO this can also be an error
                throw new RpcException($"{errMsgForUuid}. {msg}.", ErrorCode.RedundantRpc);
            }
        }

        private static void StartMining(string devs)
        {
            bool allDevices = devs == "*";
            if (allDevices) {
                startMiningAllDevices();
            } else {
                startMiningOnDeviceWithUuid(devs);
            }
        }
#endregion Start

#region Stop
        private static void stopMiningAllDevices()
        {
            var allDisabled = AvailableDevices.Devices.All(dev => dev.IsDisabled);
            if (allDisabled) {
                throw new RpcException("All devices are disabled cannot stop", ErrorCode.DisabledDevice);
            }
            var (success, msg) = ApplicationStateManager.StopAllDevice();
            if (!success) {
                throw new RpcException(msg, ErrorCode.RedundantRpc);
            }
        }

        private static void stopMiningOnDeviceWithUuid(string uuid)
        {
            string errMsgForUuid = $"Cannot stop device with uuid {uuid}";
            // get device with uuid if it exists, devs can be single device uuid
            var deviceWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(uuid);
            if (deviceWithUUID == null) {
                throw new RpcException($"{errMsgForUuid}. Device not found.", ErrorCode.NonExistentDevice);
            }
            if (deviceWithUUID.IsDisabled) {
                throw new RpcException($"{errMsgForUuid}. Device is disabled.", ErrorCode.DisabledDevice);
            }
            var (success, msg) = ApplicationStateManager.StopDevice(deviceWithUUID);
            if (!success) {
                // TODO this can also be an error
                throw new RpcException($"{errMsgForUuid}. {msg}.", ErrorCode.RedundantRpc);
            }
        }

        private static void StopMining(string devs)
        {
            bool allDevices = devs == "*";
            if (allDevices) {
                stopMiningAllDevices();
            } else {
                stopMiningOnDeviceWithUuid(devs);
            }
        }
#endregion Stop

        private static void SetPowerMode(string device, PowerLevel level)
        {
            var devs = device == "*" ? 
                AvailableDevices.Devices : 
                AvailableDevices.Devices.Where(d => d.B64Uuid == device);

            var found = devs.Count() > 0;
            var hasEnabled = false;
            var setSuccess = new List<bool>();
            foreach (var dev in devs)
            {
                if (!dev.Enabled) continue;
                if (!dev.CanSetPowerMode) continue;
                hasEnabled = true;
                // TODO check if set
                var result = dev.SetPowerMode(level);
                setSuccess.Add(result);
            }

            if (setSuccess.Any(t => t) && !setSuccess.All(t => t))
            {
                throw new RpcException("Not able to set power modes for all devices", ErrorCode.UnableToHandleRpc);
            }

            if (found && !hasEnabled)
            {
                throw new RpcException("No settable devices found", ErrorCode.UnableToHandleRpc);
            }

            if (!found)
            {
                throw new RpcException("No settable devices found", ErrorCode.UnableToHandleRpc);
            }
        }

#endregion

#region Outgoing socket calls

        public static void SetCredentials(string btc, string worker, string group)
        {
            if (CredentialValidators.ValidateBitcoinAddress(btc) && CredentialValidators.ValidateWorkerName(worker))
            {
                // Send as task since SetCredentials is called from UI threads
                Task.Factory.StartNew(() =>
                {
                    SendMinerStatus(false);
                    _socket?.StartConnection(btc, worker, group);
                });
            }
        }

        private static void SendMinerStatus(bool sendDeviceNames)
        {
            var devices = AvailableDevices.Devices;
            var rigStatus = ApplicationStateManager.CalcRigStatusString();
            var paramList = new List<JToken>
            {
                rigStatus
            };

            var deviceList = new JArray();
            foreach (var device in devices)
            {
                try
                {
                    var array = new JArray
                    {
                        sendDeviceNames ? device.Name : "",
                        device.B64Uuid  // TODO
                    };
                    var status = DeviceReportStatus(device.DeviceType, device.State);
                    array.Add(status);

                    array.Add((int)Math.Round(device.Load));

                    var speedsJson = new JArray();
                    var speeds = MiningStats.GetSpeedForDevice(device.Uuid);
                    if (speeds != null && device.State == DeviceState.Mining)
                    {
                        foreach (var kvp in speeds)
                        {
                            speedsJson.Add(new JArray((int)kvp.type, kvp.speed));
                        }
                    }
                    array.Add(speedsJson);

                    // Hardware monitoring
                    array.Add((int) Math.Round(device.Temp));
                    array.Add(device.FanSpeed);
                    array.Add((int) Math.Round(device.PowerUsage));

                    // Power mode
                    array.Add((int)device.PowerLevel);

                    // Intensity mode
                    array.Add(0);

                    deviceList.Add(array);
                }
                catch (Exception e) {
                    NHM.Common.Logger.Error("SOCKET", e.Message);
                }
            }

            paramList.Add(deviceList);

            var data = new MinerStatusMessage
            {
                param = paramList
            };
            var sendData = JsonConvert.SerializeObject(data);

            // This function is run every minute and sends data every run which has two auxiliary effects
            // Keeps connection alive and attempts reconnection if internet was dropped
            _socket?.SendData(sendData);
        }

        private static void MinerStatus_Tick(object state)
        {
            NHM.Common.Logger.Info("SOCKET", "SendMinerStatus Tick 'miner.status'");
            SendMinerStatus(false);
        }

        private static void SendExecuted(ExecutedInfo info, int? id, int code = 0, string message = null)
        {
            // First set status
            SendMinerStatus(false);
            // Then executed
            var data = new ExecutedCall(id ?? -1, code, message).Serialize();
            _socket?.SendData(data);
            // Login if we have to
            if (info?.LoginNeeded ?? false)
            {
                _socket?.StartConnection(info.NewBtc, info.NewWorker, info.NewRig);
            }
        }

#endregion

        public static void StateChanged()
        {
            SendMinerStatus(false);
        }
    }
}
#endif
