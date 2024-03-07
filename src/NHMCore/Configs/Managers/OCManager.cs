﻿//using log4net.Core;
using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Mining;
using NHMCore.Nhmws;
using NHMCore.Nhmws.V4;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NHMCore.Configs.Managers
{
    public class OCManager
    {
        private OCManager() { }
        public static OCManager Instance { get; } = new OCManager();
        private readonly string _TAG = "OCManager";
        public Task<(ErrorCode err, string msg)> ExecuteTest(string uuid, OcProfile bundle)
        {
            if (!MiningState.Instance.AnyDeviceRunning) return Task.FromResult((ErrorCode.ErrNoDeviceRunning, "No devices mining"));
            var allContainers = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .Where(d => d.State == DeviceState.Mining || d.State == DeviceState.Testing)?
                .SelectMany(d => d.AlgorithmSettings);
            if (allContainers == null || !allContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "No targets found"));

            if (bundle.AlgoId != null && bundle.MinerId != null) allContainers = allContainers.Where(d =>
                                                                                        bundle.AlgoId.Contains(d.AlgorithmName.ToLower()) &&
                                                                                        bundle.MinerId.Contains(d.PluginName.ToLower()));
            else if (bundle.AlgoId != null) allContainers = allContainers.Where(d => bundle.AlgoId.Contains(d.AlgorithmName.ToLower()));
            else if (bundle.MinerId != null) allContainers = allContainers.Where(d => bundle.MinerId.Contains(d.PluginName.ToLower()));
            if (allContainers == null || !allContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Action target mismatch, containers null"));
            var target = allContainers.Where(c => c.IsCurrentlyMining)?.FirstOrDefault();
            if(target == null)
            {
                target = allContainers.Where(c => c.Enabled)?.FirstOrDefault();
                if (target == null) return Task.FromResult((ErrorCode.TargetContainerNotFound, "Failed to switch to target algorithm container"));
            }
            target.SetTargetOcProfile(bundle, true);
            MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
        public Task<(ErrorCode err, string msg)> StopTest(string uuid, bool triggerSwitch)
        {
            var targetDeviceContainer = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .SelectMany(d => d.AlgorithmSettings)?
                .Where(a => a.IsTesting || a.ActiveOCTestProfile != null)?
                .FirstOrDefault();
            if (targetDeviceContainer == null)
            {
                Logger.Warn(_TAG, "Device not found for stop OC test");
                return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Device is not in test mode"));
            }
            targetDeviceContainer.SetTargetOcProfile(null, true);
            if(triggerSwitch) MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
        public Task<(ErrorCode err, string msg)> ApplyOcBundle(List<OcProfile> bundles)
        {
            if (bundles == null) return Task.FromResult((ErrorCode.NoError, "OcBundles == null"));
            List<AlgorithmContainer> processed = new();
            var sorted = new List<(int, OcProfile)>();
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
                var targetList = BundleManager.FindTargetGPUNames(bundle.DeviceName);
                if (targetList == null) continue;
                var current = new List<AlgorithmContainer>();
                if (type == 0) current = AvailableDevices.Devices
                        .Where(d => targetList.Contains(d.Name.ToLower()))?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.AlgoId.Contains(c.AlgorithmName.ToLower()))?
                        .Where(c => bundle.MinerId.Contains(c.PluginName.ToLower()))?
                        .ToList();
                else if (type == 1) current = AvailableDevices.Devices
                        .Where(d => targetList.Contains(d.Name.ToLower()))?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.AlgoId.Contains(c.AlgorithmName.ToLower()))?
                        .ToList();
                else if (type == 2) current = AvailableDevices.Devices
                        .Where(d => targetList.Contains(d.Name.ToLower()))?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .Where(c => bundle.MinerId.Contains(c.PluginName.ToLower()))?
                        .ToList();
                else current = AvailableDevices.Devices
                        .Where(d => targetList.Contains(d.Name.ToLower()))?
                        .SelectMany(d => d.AlgorithmSettings)?
                        .ToList();
                if (current == null) continue;
                current = current.Where(c => !processed.Contains(c)).ToList();
                processed.AddRange(current);
                foreach (var container in current)
                {
                    Logger.Warn(_TAG, $"\t{container.ComputeDevice.ID}-{container.ComputeDevice.Name}/{container.AlgorithmName}/{container.PluginName}");
                    container.SetTargetOcProfile(bundle, false);
                }
            }
            MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }

        public Task ResetOcBundle(bool triggerSwitch = true)
        {
            var containers = AvailableDevices.Devices.SelectMany(d => d.AlgorithmSettings);
            foreach (var container in containers)
            {
                container.SetTargetOcProfile(null, false);
            }
            if (triggerSwitch) MiningManager.TriggerSwitchCheck();
            return Task.FromResult((ErrorCode.NoError, "Success"));
        }
    }
}
