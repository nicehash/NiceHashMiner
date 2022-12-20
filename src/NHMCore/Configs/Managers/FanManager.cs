using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Mining;
using NHMCore.Nhmws;
using NHMCore.Nhmws.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs.Managers
{
    public class FanManager
    {
        private FanManager() { }
        public static FanManager Instance { get; } = new FanManager();
        private readonly string _TAG = "FanManager";

        public enum FanReturn
        {
            Success,
            PartialSuccess,
            Fail
        }

        public Task<(ErrorCode err, string msg)> ExecuteTest(string uuid, FanBundle bundle)
        {
            if (!MiningState.Instance.AnyDeviceRunning) return Task.FromResult((ErrorCode.ErrNoDeviceRunning, "No devices mining"));
            var allContainers = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .Where(d => d.State == DeviceState.Mining || d.State == DeviceState.Testing)?
                .SelectMany(d => d.AlgorithmSettings);
            if (allContainers == null || !allContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "No targets found"));

            List<AlgorithmContainer> specificContainers = allContainers.ToList();
            if (bundle.AlgoId != null && bundle.MinerId != null) specificContainers = allContainers.Where(d =>
                                                                                        bundle.AlgoId.Contains(d.AlgorithmName) &&
                                                                                        bundle.MinerId.Contains(d.PluginName))?.ToList();
            else if (bundle.AlgoId != null) specificContainers = allContainers.Where(d => bundle.AlgoId.Contains(d.AlgorithmName))?.ToList();
            else if (bundle.MinerId != null) specificContainers = allContainers.Where(d => bundle.MinerId.Contains(d.PluginName))?.ToList();
            if (specificContainers == null || !specificContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Action target mismatch, containers null"));
            var target = specificContainers.Where(c => c.IsCurrentlyMining)?.FirstOrDefault();
            if (target == null)
            {
                target = specificContainers.FirstOrDefault();
                if (target == null) return Task.FromResult((ErrorCode.TargetContainerNotFound, "Failed to switch to target algorithm container"));
            }
            //AvailableDevices.Devices //if we want switching for loose options we can set true to specific containers in the future
            //    .Where(d => d.B64Uuid == uuid)?
            //    .SelectMany(d => d.AlgorithmSettings)?
            //    .ToList()?
            //    .ForEach(c => c.IsTesting = false);
            target.SetTargetFanProfile(bundle, true);
            MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
        public Task<(ErrorCode err, string msg)> StopTest(string uuid, bool triggerSwitch)
        {
            var targetDeviceContainer = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .SelectMany(d => d.AlgorithmSettings)?
                .Where(a => a.IsTesting || a.ActiveFanTestProfile != null)?
                .FirstOrDefault();
            if (targetDeviceContainer == null)
            {
                Logger.Error(_TAG, "Device not found for stop OC test");
                return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Device not found"));
            }
            if(triggerSwitch) targetDeviceContainer.SetTargetFanProfile(null, true);
            MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
        public Task<(ErrorCode err, string msg)> ApplyFanBundle(List<FanBundle> bundles)
        {
            if (bundles == null) return Task.FromResult((ErrorCode.NoError, "FanBundles == null"));
            List<AlgorithmContainer> processed = new();
            var sorted = new List<(int, FanBundle)>();
            foreach (var bundle in bundles)
            {
                if (bundle.MinerId != null && bundle.AlgoId != null) sorted.Add((0, bundle));
                else if (bundle.MinerId == null && bundle.AlgoId != null) sorted.Add((1, bundle));
                else if (bundle.MinerId != null && bundle.AlgoId == null) sorted.Add((2, bundle));
                else sorted.Add((3, bundle));
            }
            sorted = sorted.OrderBy(item => item.Item1).ToList();
            foreach (var (type, bundle) in sorted)
            {
                var current = new List<AlgorithmContainer>();
                if (type == 0) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.AlgoId.Contains(c.AlgorithmName))?
                        .Where(c => bundle.MinerId.Contains(c.PluginName))?
                        .ToList();
                else if (type == 1) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.AlgoId.Contains(c.AlgorithmName))?
                        .ToList();
                else if (type == 2) current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.MinerId.Contains(c.PluginName))?
                        .ToList();
                else current = AvailableDevices.Devices
                        .Where(d => d.Name == bundle.DeviceName)?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .ToList();
                if (current == null) continue;
                current = current.Where(c => !processed.Contains(c)).ToList();
                processed.AddRange(current);
                foreach (var container in current)
                {
                    Logger.Warn(_TAG, $"\t{container.ComputeDevice.ID}-{container.ComputeDevice.Name}/{container.AlgorithmName}/{container.PluginName}");
                    container.SetTargetFanProfile(bundle, false);
                }
            }
            MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }

        public Task ResetFanBundle(bool triggerSwitch = true)
        {
            var containers = AvailableDevices.Devices.SelectMany(d => d.AlgorithmSettings);
            foreach (var container in containers)
            {
                container.SetTargetFanProfile(null, false);
            }
            if (triggerSwitch) MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
    }
}
