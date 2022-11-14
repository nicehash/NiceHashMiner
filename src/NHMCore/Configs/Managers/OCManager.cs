using NHMCore.ApplicationState;
using NHMCore.Mining;
using NHMCore.Nhmws.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs.Managers
{
    public class OCManager
    {
        private OCManager() { }
        public static OCManager Instance { get; } = new OCManager();
        private List<OcBundle> _ocBundles = new();
        public void ClearBundles()
        {
            _ocBundles.Clear();
        }
        public bool AddOCAndApply(OcBundle bundle)
        {
            _ocBundles.Add(bundle);
            if (!MiningState.Instance.AnyDeviceRunning) return true;
            //var miningDevs = AvailableDevices.Devices.Where(d => d.State == NHM.Common.Enums.DeviceState.Mining && d.State == NHM.Common.Enums.DeviceState.Benchmarking);
            //if (!miningDevs.Any()) return true;
            if ((bundle.AlgoId == null && bundle.MinerId == null) || (!bundle.AlgoId.Any() && !bundle.MinerId.Any()))
            {
                var contextDevices = AvailableDevices.Devices
                    .Where(d => d.State == NHM.Common.Enums.DeviceState.Mining || d.State == NHM.Common.Enums.DeviceState.Benchmarking)
                    .Where(d => d.Name == bundle.DeviceName);
                foreach (var contextDevice in contextDevices)
                {
                    var okCC = contextDevice.SetCoreClock(bundle.CoreClock);
                    var okMC = contextDevice.SetMemoryClock(bundle.MemoryClock);
                    var okTDP = contextDevice.SetPowerModeManual(bundle.TDP);
                    //todo continue here
                    //foreach(var algoContainer in contextDevice.AlgorithmSettings)
                    //{
                    //    //stuff can be null here!!!!!

                    //    algoContainer.OCSetting = (bundle.CoreClock, bundle.MemoryClock, bundle.MemoryClock);
                    //    //apply
                    //}
                }
            }
            else if(bundle.AlgoId == null)
            {

            }
            else if(bundle.MinerId == null)
            {

            }
            return true;

        }
    }
}
