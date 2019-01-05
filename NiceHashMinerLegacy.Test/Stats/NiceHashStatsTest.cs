using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Stats;
using NiceHashMiner.Stats.Models;
using System.Linq;

namespace NiceHashMinerLegacy.Tests.Stats
{
    [TestClass]
    public class NiceHashStatsTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            ComputeDeviceManager.InitFakeDevs();
        }

        [TestMethod]
        public void VersionUpdateShouldParse()
        {
            NiceHashStats.ProcessData(TestSocketCalls.Essentials, out var ex, out var id);

            Assert.AreEqual("1.9.1.2", NiceHashStats.Version);
            Assert.IsFalse(ex);
            Assert.IsNull(id);
        }

        [TestMethod]
        [ExpectedException(typeof(RpcException))]
        public void SetInvalidWorkerShouldThrow()
        {
            NiceHashStats.ProcessData(TestSocketCalls.InvalidWorkerSet, out _, out _);
        }

        [TestMethod]
        public void SetValidWorkerShouldPrase()
        {
            var set = NiceHashStats.ProcessData(TestSocketCalls.ValidWorkerSet, out var ex, out var id);
            Assert.IsTrue(set.LoginNeeded);
            Assert.AreEqual("main", set.NewWorker);
            Assert.AreEqual("main", ConfigManager.GeneralConfig.WorkerName);
            Assert.IsTrue(ex);
            Assert.AreEqual(12, id);
        }

        [TestMethod]
        [ExpectedException(typeof(RpcException))]
        public void SetInvalidUserShouldThrow()
        {
            NiceHashStats.ProcessData(TestSocketCalls.InvalidUserSet, out _, out _);
        }

        [TestMethod]
        public void SetValidUserShouldParse()
        {
            var ex = NiceHashStats.ProcessData(TestSocketCalls.ValidUserSet, out var e, out var id);
            Assert.IsTrue(ex.LoginNeeded);
            Assert.AreEqual("3KpWmp49Cdbswr23KhjagNbwqiwcFh8Br2", ex.NewBtc);
            Assert.AreEqual("3KpWmp49Cdbswr23KhjagNbwqiwcFh8Br2", ConfigManager.GeneralConfig.BitcoinAddress);
            Assert.IsTrue(e);
            Assert.AreEqual(15, id);
        }

        [TestMethod]
        public void EnableDevicesShouldMatch()
        {
            var devs = ComputeDeviceManager.Available.Devices;
            // Start all false
            foreach (var dev in devs)
            {
                dev.Enabled = false;
            }

            var first = devs.First();

            NiceHashStats.ProcessData(string.Format(TestSocketCalls.EnableOne, first.B64Uuid), out var e, out var id);

            Assert.IsTrue(e);
            Assert.AreEqual(89, id);

            foreach (var dev in devs)
            {
                if (dev.B64Uuid != first.B64Uuid)
                    Assert.IsFalse(dev.Enabled);
                else
                    Assert.IsTrue(dev.Enabled);
            }

            NiceHashStats.ProcessData(TestSocketCalls.EnableAll, out e, out id);

            Assert.IsTrue(e);
            Assert.AreEqual(89, id);

            Assert.IsTrue(devs.All(d => d.Enabled));

            var last = devs.Last();

            NiceHashStats.ProcessData(string.Format(TestSocketCalls.DisableOne, last.B64Uuid), out e, out id);

            Assert.IsTrue(e);
            Assert.AreEqual(89, id);

            foreach (var dev in devs)
            {
                if (dev.B64Uuid != last.B64Uuid)
                    Assert.IsTrue(dev.Enabled);
                else
                    Assert.IsFalse(dev.Enabled);
            }

            NiceHashStats.ProcessData(TestSocketCalls.DisableAll, out e, out id);

            Assert.IsTrue(e);
            Assert.AreEqual(89, id);

            Assert.IsFalse(devs.Any(d => d.Enabled));
        }

        [TestMethod]
        [ExpectedException(typeof(RpcException))]
        public void InvalidEnableShouldThrow()
        {
            NiceHashStats.ProcessData(TestSocketCalls.InvalidEnableOne, out _, out _);
        }

        [TestMethod]
        [ExpectedException(typeof(RpcException))]
        public void InvalidDisableShouldThrow()
        {
            NiceHashStats.ProcessData(TestSocketCalls.InvalidDisableOne, out _, out _);
        }
    }
}
