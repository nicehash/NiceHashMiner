using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Miners;

namespace NiceHashMinerLegacy.Tests.Miners
{
    [TestClass]
    public class MinerApiWatchdogTest
    {
        [TestMethod]
        public void GroupTimeoutFalseTest()
        {
            MinerApiWatchdog.Clear();
            var minuteTimeout = new TimeSpan(0, 1, 0);
            var addTime = DateTime.UtcNow;
            var timestampTime = addTime.Add(new TimeSpan(0, 0, 35));
            const string groupKey = "group1";
            MinerApiWatchdog.AddGroup(groupKey, minuteTimeout, addTime);
            MinerApiWatchdog.UpdateApiTimestamp(groupKey, timestampTime);
            var checkTime = addTime.Add(new TimeSpan(0, 0, 50));
            CollectionAssert.DoesNotContain(MinerApiWatchdog.GetTimedoutGroups(checkTime), groupKey);
        }

        [TestMethod]
        public void GroupTimeoutTrueTest()
        {
            MinerApiWatchdog.Clear();
            var minuteTimeout = new TimeSpan(0, 1, 0);
            var addTime = DateTime.UtcNow;
            var timestampTime = addTime.Add(new TimeSpan(0, 0, 30));
            const string groupKey = "group1";
            MinerApiWatchdog.AddGroup(groupKey, minuteTimeout, addTime);
            MinerApiWatchdog.UpdateApiTimestamp(groupKey, timestampTime);
            var checkTime = addTime.Add(new TimeSpan(0, 1, 30));
            CollectionAssert.Contains(MinerApiWatchdog.GetTimedoutGroups(checkTime), groupKey);
        }

        [TestMethod]
        public void Combination01Test()
        {
            MinerApiWatchdog.Clear();
            // TODO
        }
    }
}
