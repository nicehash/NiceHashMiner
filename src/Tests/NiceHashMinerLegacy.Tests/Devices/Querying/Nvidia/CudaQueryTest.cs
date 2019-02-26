using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Devices.Querying.Nvidia;

namespace NiceHashMinerLegacy.Tests.Devices.Querying.Nvidia
{
    [TestClass]
    public class CudaQueryTest
    {
        private class CudaQueryDummy : CudaQuery
        {
            protected override string GetCudaQueryString()
            {
                return @"{""CudaDevices"":[{""DeviceGlobalMemory"":11811160064,""DeviceID"":0,""DeviceName"":""GeForce GTX 1080 Ti"",""HasMonitorConnected"":0,""SMX"":28,""SM_major"":6,""SM_minor"":1,""UUID"":""GPU-5f6a9bb5-6d82-22d7-5d3f-4e11f4fa9576"",""VendorID"":14402,""VendorName"":""EVGA"",""pciBusID"":1,""pciDeviceId"":453382366,""pciSubSystemId"":1721120834},{""DeviceGlobalMemory"":11811160064,""DeviceID"":1,""DeviceName"":""GeForce GTX 1080 Ti"",""HasMonitorConnected"":1,""SMX"":28,""SM_major"":6,""SM_minor"":1,""UUID"":""GPU-1590f69d-2d74-d4a7-9520-eacd1431971d"",""VendorID"":14402,""VendorName"":""EVGA"",""pciBusID"":2,""pciDeviceId"":453382366,""pciSubSystemId"":1721120834}],""DriverVersion"":""417.71"",""ErrorString"":""""}";
            }
        }

        [TestMethod]
        public void QueryDevices_ShouldMatch()
        {
            var query = new CudaQueryDummy();
            var success = query.TryQueryCudaDevices(out var cudaDevs);

            Assert.IsTrue(success);

            Assert.AreEqual(2, cudaDevs.Count);
        }
    }
}
