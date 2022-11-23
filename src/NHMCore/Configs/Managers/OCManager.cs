//using log4net.Core;
using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Mining;
using NHMCore.Nhmws;
using NHMCore.Nhmws.V4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public enum OcReturn
        {
            Success,
            PartialSuccess,
            Fail
        }

        public Task<(ErrorCode err, string msg)> ExecuteTest(string uuid, OcBundle bundle)
        {
            if (!MiningState.Instance.AnyDeviceRunning) return Task.FromResult((ErrorCode.ErrNoDeviceRunning, "No devices mining"));
            var allContainers = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
                .SelectMany(d => d.AlgorithmSettings);
            if (allContainers == null || !allContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "No targets found")); //if mine anything

            List<AlgorithmContainer> specificContainers = allContainers.ToList();
            if (bundle.AlgoId != null && bundle.MinerId != null) specificContainers = allContainers.Where(d =>
                                                                                        bundle.AlgoId.Contains(d.AlgorithmName) &&
                                                                                        bundle.MinerId.Contains(d.PluginName))?.ToList();//if both algo and miner
            else if (bundle.AlgoId != null) specificContainers = allContainers.Where(d => bundle.AlgoId.Contains(d.AlgorithmName))?.ToList(); //if only algo
            else if (bundle.MinerId != null) specificContainers = allContainers.Where(d => bundle.MinerId.Contains(d.PluginName))?.ToList(); //if only miner
            if (specificContainers == null || !specificContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Action target mismatch, containers null"));
            var target = specificContainers.Where(c => c.IsCurrentlyMining)?.FirstOrDefault();
            if(target != null)
            {
                target.ForceMiningOnlyThisContainer = true;
                Logger.Warn(_TAG, $"\t{target.ComputeDevice.ID}-{target.ComputeDevice.Name}/{target.AlgorithmName}/{target.PluginName}");
            }
            else
            {
                target = specificContainers.FirstOrDefault();
                if (target == null) return Task.FromResult((ErrorCode.TargetContainerNotFound, "Failed to switch to target algorithm container"));
                target.ForceMiningOnlyThisContainer = true;
            }
            var ret = target.SetOcForDevice(bundle);
            if (ret.Result == OcReturn.Success) return Task.FromResult((ErrorCode.NoError, "Successfully applied test"));
            if (ret.Result == OcReturn.Fail)
            {
                target.ForceMiningOnlyThisContainer = false;
                return Task.FromResult((ErrorCode.TestTotalFail, "Failed to apply test"));
            }
            return Task.FromResult((ErrorCode.TestPartialFail, "Partially applied test"));
        }
        public Task<(ErrorCode err, string msg)> StopTest(string uuid)
        {
            var targetDeviceContainer = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .SelectMany(d => d.AlgorithmSettings)?
                .Where(a => a.IsTesting)?
                .FirstOrDefault();
            if (targetDeviceContainer == null)
            {
                Logger.Error(_TAG, "Device not found for stop OC test");
                return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Device not found"));
            }
            var ret = targetDeviceContainer.ResetOcForDevice();
            targetDeviceContainer.ForceMiningOnlyThisContainer = false;
            if (ret.Result == OcReturn.Success) return Task.FromResult((ErrorCode.NoError, "Successfully stopped test"));
            if (ret.Result == OcReturn.Fail) return Task.FromResult((ErrorCode.TestTotalFail, "Failed to stop test"));
            return Task.FromResult((ErrorCode.TestPartialFail, "Stopped test"));
        }
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
                    container.SetOcForDevice(bundle);
                }
            }
            return Task.CompletedTask;
        }
    }
}
