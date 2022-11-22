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

        public enum OcReturn
        {
            Success,
            PartialSuccess,
            Fail
        }

        //todo cleanup for many different tests if they come one after another
        //clear first?
        public Task<(ErrorCode err, string msg)> ExecuteTest(string uuid, OcBundle bundle)
        {
            //find all algo containers for device with uuid
            //find if specific container exists
            //  if yes
            //      if already mining
            //          apply benchmark
            //          disable switch for this device
            //      if mining something else
            //          switch to target
            //          disable switch for this device
            //  if no
            //      return error



            if (!MiningState.Instance.AnyDeviceRunning) return Task.FromResult((ErrorCode.ErrNoDeviceRunning, "No devices running"));
            var allContainers = AvailableDevices.Devices
                .Where(d => d.B64Uuid == uuid)?
                .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
                //.Where(d => d.Name == bundle.DeviceName)?
                .SelectMany(d => d.AlgorithmSettings);
            if (allContainers == null || !allContainers.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "No targets found")); //if mine anything

            List<AlgorithmContainer> specificContainers = new List<AlgorithmContainer>();
            //in this filtering we can leave out stuff that we mine and is not in target 
            if (bundle.AlgoId != null && bundle.MinerId != null) specificContainers = allContainers.Where(d =>
                                                                                        bundle.AlgoId.Contains(d.AlgorithmName) &&
                                                                                        bundle.MinerId.Contains(d.PluginName))?.ToList();//if both algo and miner
            else if (bundle.AlgoId != null) specificContainers = allContainers.Where(d => bundle.AlgoId.Contains(d.AlgorithmName))?.ToList(); //if only algo
            else if (bundle.MinerId != null) specificContainers = allContainers.Where(d => bundle.MinerId.Contains(d.PluginName))?.ToList(); //if only miner
            if (specificContainers == null || !specificContainers.Any())
            {
                return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Action target mismatch, containers null"));
            }
            var distinctDevs = specificContainers.Where(c => c.IsCurrentlyMining)?.DistinctBy(d => d.ComputeDevice.Uuid) ?? new List<AlgorithmContainer>();
            Logger.Info(_TAG, "Applying OC Test for following containers:");
            List<OcReturn> success = new();

            var currentlyMiningContainer = distinctDevs.Where(c => c.IsCurrentlyMining)?.FirstOrDefault();
            if(currentlyMiningContainer != null)  //if one of target containers already mining leave it
            {
                var ret = currentlyMiningContainer.SetOcForDevice(bundle);
                if (ret.IsCompleted) success.Add(ret.Result);
                Logger.Warn(_TAG, $"\t{currentlyMiningContainer.ComputeDevice.ID}-{currentlyMiningContainer.ComputeDevice.Name}/{currentlyMiningContainer.AlgorithmName}/{currentlyMiningContainer.PluginName}");
                //stop switching 4 this device !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
            else
            {
                //switch to one of target
            }
            foreach (var container in distinctDevs) //todo run only once
            {
                Logger.Warn(_TAG, $"\t{container.ComputeDevice.ID}-{container.ComputeDevice.Name}/{container.AlgorithmName}/{container.PluginName}");
                var ret = container.SetOcForDevice(bundle);
            }
            //if one of the target containers not mining switch to one that is


            if (success.All(s => s == OcReturn.Success)) return Task.FromResult((ErrorCode.NoError, "Successfully applied test"));
            if (success.All(s => s == OcReturn.Fail)) return Task.FromResult((ErrorCode.TestTotalFail, "Failed to apply test"));
            return Task.FromResult((ErrorCode.TestPartialFail, "Partially applied test"));



            //if (!MiningState.Instance.AnyDeviceRunning) return Task.FromResult((ErrorCode.ErrNoDeviceRunning, "No devices running"));
            //var allContainersGeneral = AvailableDevices.Devices
            //    .Where(d => d.B64Uuid == uuid)?
            //    .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)?
            //    //.Where(d => d.Name == bundle.DeviceName)?
            //    .SelectMany(d => d.AlgorithmSettings);

            //if (allContainersGeneral == null || !allContainersGeneral.Any()) return Task.FromResult((ErrorCode.TargetDeviceNotFound, "No targets found")); //if mine anything
            //List<AlgorithmContainer> allContainers = new List<AlgorithmContainer>();
            //if (bundle.AlgoId != null && bundle.MinerId != null) allContainers = allContainersGeneral.Where(d =>  
            //                                                                            bundle.AlgoId.Contains(d.AlgorithmName) && 
            //                                                                            bundle.MinerId.Contains(d.PluginName))?.ToList();//if both algo and miner
            //else if (bundle.AlgoId != null) allContainers = allContainers.Where(d => bundle.AlgoId.Contains(d.AlgorithmName))?.ToList(); //if only algo
            //else if (bundle.MinerId != null) allContainers = allContainers.Where(d => bundle.MinerId.Contains(d.PluginName))?.ToList(); //if only miner
            //if (allContainers == null || !allContainers.Any())
            //{
            //    return Task.FromResult((ErrorCode.TargetDeviceNotFound, "Action target mismatch, containers null"));
            //}
            //var distinctDevs = allContainers.Where(c => c.IsCurrentlyMining)?.DistinctBy(d => d.ComputeDevice.Uuid) ?? new List<AlgorithmContainer>();
            //Logger.Info(_TAG, "Applying OC Test for following containers:");
            //List<OcReturn> success = new();
            //foreach (var container in distinctDevs) //todo run only once
            //{
            //    Logger.Warn(_TAG, $"\t{container.ComputeDevice.ID}-{container.ComputeDevice.Name}/{container.AlgorithmName}/{container.PluginName}");
            //    var ret = container.SetOcForDevice(bundle);
            //    if (ret.IsCompleted) success.Add(ret.Result);
            //}
            //if (success.All(s => s == OcReturn.Success)) return Task.FromResult((ErrorCode.NoError, "Successfully applied test"));
            //if (success.All(s => s == OcReturn.Fail)) return Task.FromResult((ErrorCode.TestTotalFail, "Failed to apply test")); 
            //return Task.FromResult((ErrorCode.TestPartialFail, "Partially applied test"));
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
            if (ret.Result == OcReturn.Success) return Task.FromResult((ErrorCode.NoError, "Successfully stopped test"));
            if (ret.Result == OcReturn.Fail) return Task.FromResult((ErrorCode.TestTotalFail, "Failed to stop test"));
            return Task.FromResult((ErrorCode.TestPartialFail, "Stopped test"));
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
                    container.SetOcForDevice(bundle);
                }
            }
            return Task.CompletedTask;
        }
    }
}
