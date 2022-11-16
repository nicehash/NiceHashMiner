using NHM.Common;
using NHMCore.ApplicationState;
using NHMCore.Mining;
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
            var setCC = bundle.CoreClock == 0 ? container.ComputeDevice.SetCoreClock(bundle.CoreClock) : false;
            Logger.Warn(_TAG, $"Setting core clock for device {container.ComputeDevice.Name} success: {setCC}");
            var setMC = bundle.MemoryClock == 0 ? container.ComputeDevice.SetMemoryClock(bundle.MemoryClock) : false;
            Logger.Warn(_TAG, $"Setting memory clock for device {container.ComputeDevice.Name} success: {setMC}");
            var setTDP = bundle.TDP == 0 ? container.ComputeDevice.SetPowerModeManual(bundle.TDP) : false;
            Logger.Warn(_TAG, $"Setting TDP for device {container.ComputeDevice.Name} success: {setTDP}");
            if (setCC && setMC && setTDP) return Task.FromResult(OcReturn.Success);
            if (!setCC && !setMC && !setTDP) return Task.FromResult(OcReturn.Fail);
            return Task.FromResult(OcReturn.PartialSuccess);
        }


        //todo cleanup for many different tests if they come one after another
        public Task<string> ExecuteTest(string uuid, OcBundle bundle)
        {
            _testOcBundles.Add(bundle);
            if (!MiningState.Instance.AnyDeviceRunning) return Task.FromResult("No devices running");
            var allContainers = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
                .Where(d => d.Name == bundle.DeviceName)?
                .SelectMany(d => d.AlgorithmSettings);
            //var MiningState = AvailableDevices.Devices.Where(d => d.Name == bundle.DeviceName)?.SelectMany(d => d.AlgorithmSettings); //for testing
            if (allContainers == null || !allContainers.Any()) return Task.FromResult("Action target mismatch");
            if (bundle.AlgoId != null && bundle.MinerId != null) allContainers = allContainers.Where(d => 
                                                                                        bundle.AlgoId.Contains(d.AlgorithmName) && 
                                                                                        bundle.MinerId.Contains(d.PluginName));
            else if (bundle.AlgoId != null) allContainers = allContainers.Where(d => bundle.AlgoId.Contains(d.AlgorithmName));
            else if (bundle.MinerId != null) allContainers = allContainers.Where(d => bundle.MinerId.Contains(d.PluginName));
            if (allContainers == null || !allContainers.Any()) return Task.FromResult("Action target mismatch, containers null");
            var distinctDevs = allContainers.DistinctBy(d => d.ComputeDevice.Uuid);
            Logger.Info(_TAG, "Applying OC Test for following containers:");
            List<OcReturn> success = new();
            foreach (var container in distinctDevs)
            {
                Logger.Warn(_TAG, $"\t{container.ComputeDevice.ID}-{container.ComputeDevice.Name}/{container.AlgorithmName}/{container.PluginName}");
                var ret = SetOcForDevice(container, bundle);
                if (ret.IsCompleted) success.Add(ret.Result);
            }
            if (success.All(s => s == OcReturn.Success)) return Task.FromResult("Successfully executed test");
            if (success.All(s => s == OcReturn.Fail)) return Task.FromResult("Failed to execute all tests");
            return Task.FromResult("Partially executed tests");
        }
        //clear first????
        public Task ApplyOcBundle(List<OcBundle> bundles) //tdp is null? can a value be missing??????
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
