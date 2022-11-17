//using log4net.Core;
using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Mining;
using NHMCore.Nhmws;
using NHMCore.Nhmws.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs.Managers
{
    public class OCManager
    {
        private OCManager() { }
        public static OCManager Instance { get; } = new OCManager();
        private List<OcBundle> _ocBundles = new();
        private List<OcBundle> _testOcBundles = new();
        private readonly string _TAG = "OCManager";

        private enum OcReturn
        {
            Success,
            PartialSuccess,
            Fail
        }

        private Task<OcReturn> SetOcForDevice(AlgorithmContainer container, OcBundle bundle)
        {
            var setCC = bundle.CoreClock != 0 ? container.ComputeDevice.SetCoreClock(bundle.CoreClock) : true;
            Logger.Warn(_TAG, $"Setting core clock for device {container.ComputeDevice.Name} success: {setCC}");
            var setMC = bundle.MemoryClock != 0 ? container.ComputeDevice.SetMemoryClock(bundle.MemoryClock) : true;
            Logger.Warn(_TAG, $"Setting memory clock for device {container.ComputeDevice.Name} success: {setMC}");
            var setTDP = bundle.TDP != 0 ? container.ComputeDevice.SetPowerModeManual(bundle.TDP) : true;
            Logger.Warn(_TAG, $"Setting TDP for device {container.ComputeDevice.Name} success: {setTDP}");
            var ret = OcReturn.Success;
            if (setCC && setMC && setTDP) ret = OcReturn.Success;
            else if (!setCC && !setMC && !setTDP) ret = OcReturn.Fail;
            else ret = OcReturn.PartialSuccess;
            if (ret == OcReturn.Success || ret == OcReturn.PartialSuccess) container.ComputeDevice.State = DeviceState.Testing;
            return Task.FromResult(ret);
        }


        //todo cleanup for many different tests if they come one after another
        //clear first?
        public Task<(ErrorCode err, string msg)> ExecuteTest(string uuid, OcBundle bundle)
        {
            _testOcBundles.Add(bundle);
            if (!MiningState.Instance.AnyDeviceRunning) return Task.FromResult((ErrorCode.ErrNoDeviceRunning, "No devices running"));
            var allContainers = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
                .Where(d => d.Name == bundle.DeviceName)?
                .SelectMany(d => d.AlgorithmSettings);
            //var MiningState = AvailableDevices.Devices.Where(d => d.Name == bundle.DeviceName)?.SelectMany(d => d.AlgorithmSettings); //for testing
            if (allContainers == null || !allContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "No targets found"));
            if (bundle.AlgoId != null && bundle.MinerId != null) allContainers = allContainers.Where(d => 
                                                                                        bundle.AlgoId.Contains(d.AlgorithmName) && 
                                                                                        bundle.MinerId.Contains(d.PluginName));
            else if (bundle.AlgoId != null) allContainers = allContainers.Where(d => bundle.AlgoId.Contains(d.AlgorithmName));
            else if (bundle.MinerId != null) allContainers = allContainers.Where(d => bundle.MinerId.Contains(d.PluginName));
            if (allContainers == null || !allContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Action target mismatch, containers null"));
            var distinctDevs = allContainers.DistinctBy(d => d.ComputeDevice.Uuid);
            Logger.Info(_TAG, "Applying OC Test for following containers:");
            List<OcReturn> success = new();
            foreach (var container in distinctDevs)
            {
                Logger.Warn(_TAG, $"\t{container.ComputeDevice.ID}-{container.ComputeDevice.Name}/{container.AlgorithmName}/{container.PluginName}");
                var ret = SetOcForDevice(container, bundle);
                if (ret.IsCompleted) success.Add(ret.Result);
            }
            if (success.All(s => s == OcReturn.Success)) return Task.FromResult((ErrorCode.NoError, "Successfully applied test"));
            if (success.All(s => s == OcReturn.Fail)) return Task.FromResult((ErrorCode.TestApplyTotalFail, "Failed to apply test")); 
            return Task.FromResult((ErrorCode.TestApplyPartialPartial, "Partially applied test"));
        }
        //clear first????
        public Task ApplyOcBundle(List<OcBundle> bundles)
        {
            _ocBundles.AddRange(bundles);
            List<AlgorithmContainer> processed = new();
            if (!MiningState.Instance.AnyDeviceRunning) return Task.CompletedTask;
            var sorted = new List<(int, OcBundle)>();
            foreach (var bundle in bundles)
            {
                if (bundle.MinerId.Any() && bundle.AlgoId.Any()) sorted.Add((0, bundle));
                else if (!bundle.MinerId.Any() && bundle.AlgoId.Any()) sorted.Add((1, bundle));
                else if (bundle.MinerId.Any() && !bundle.AlgoId.Any()) sorted.Add((2, bundle));
                else sorted.Add((3, bundle));
            }
            sorted = sorted.OrderByDescending(item => item.Item1).Reverse().ToList();
            foreach (var (type, bundle) in sorted)
            {
                var current = new List<AlgorithmContainer>();
                if (type == 0) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.AlgoId.Contains(c.AlgorithmName))?
                        .Where(c => bundle.MinerId.Contains(c.PluginName))?
                        .ToList();
                else if (type == 1) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.AlgoId.Contains(c.AlgorithmName))?
                        .ToList();
                else if (type == 2) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.MinerId.Contains(c.PluginName))?
                        .ToList();
                else current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .ToList();
                if (current == null) continue;
                current = current.Where(c => !processed.Contains(c)).ToList();
                processed.AddRange(current);
                foreach (var container in current)
                {
                    Logger.Warn(_TAG, $"\t{container.ComputeDevice.ID}-{container.ComputeDevice.Name}/{container.AlgorithmName}/{container.PluginName}");
                    SetOcForDevice(container, bundle);
                }
            }
            return Task.CompletedTask;
        }
    }
}
