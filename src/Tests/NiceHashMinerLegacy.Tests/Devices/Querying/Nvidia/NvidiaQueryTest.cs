using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Devices.Querying.Nvidia;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMinerLegacy.Tests.Devices.Querying.Nvidia
{
    [TestClass]
    public class NvidiaQueryTest
    {
        private class NvidiaQueryDummy : NvidiaQuery
        {
            private class CudaQueryDummy : CudaQuery
            {
                protected override string GetCudaQueryString()
                {
                    return @"{""CudaDevices"":[{""DeviceGlobalMemory"":11811160064,""DeviceID"":0,""DeviceName"":""GeForce GTX 1080 Ti"",""HasMonitorConnected"":0,""SMX"":28,""SM_major"":6,""SM_minor"":1,""UUID"":""GPU-5f6a9bb5-6d82-22d7-5d3f-4e11f4fa9576"",""VendorID"":14402,""VendorName"":""EVGA"",""pciBusID"":1,""pciDeviceId"":453382366,""pciSubSystemId"":1721120834},{""DeviceGlobalMemory"":11811160064,""DeviceID"":1,""DeviceName"":""GeForce GTX 1080 Ti"",""HasMonitorConnected"":1,""SMX"":28,""SM_major"":6,""SM_minor"":1,""UUID"":""GPU-1590f69d-2d74-d4a7-9520-eacd1431971d"",""VendorID"":14402,""VendorName"":""EVGA"",""pciBusID"":2,""pciDeviceId"":453382366,""pciSubSystemId"":1721120834}],""DriverVersion"":""417.71"",""ErrorString"":""""}";
                }
            }

            public NvidiaQueryDummy()
            {
                CudaQuery = new CudaQueryDummy();
            }
        }

        [TestMethod]
        public void QueryDevices_ShouldMatch()
        {
            var query = new NvidiaQueryDummy();
            var cudaDevs = query.QueryCudaDevices();

            Assert.AreEqual(2, cudaDevs.Count);

            foreach (var dev in cudaDevs)
            {
                Assert.AreEqual("EVGA GeForce GTX 1080 Ti", dev.Name);
                Assert.AreEqual(DeviceGroupType.NVIDIA_6_x, dev.DeviceGroupType);
                Assert.IsTrue(dev.IsEtherumCapale);
                Assert.AreEqual(DeviceType.NVIDIA, dev.DeviceType);
                Assert.AreEqual(11811160064ul, dev.GpuRam);
                Assert.IsTrue(dev.ShouldRunEthlargement);
            }

            Assert.AreEqual(0, cudaDevs[0].ID);
            Assert.AreEqual(0, cudaDevs[0].IDByBus);
            Assert.AreEqual(1, cudaDevs[0].BusID);
            Assert.AreEqual("GPU-5f6a9bb5-6d82-22d7-5d3f-4e11f4fa9576", cudaDevs[0].Uuid);

            Assert.AreEqual(1, cudaDevs[1].ID);
            Assert.AreEqual(1, cudaDevs[1].IDByBus);
            Assert.AreEqual(2, cudaDevs[1].BusID);
            Assert.AreEqual("GPU-1590f69d-2d74-d4a7-9520-eacd1431971d", cudaDevs[1].Uuid);
        }
    }
}
