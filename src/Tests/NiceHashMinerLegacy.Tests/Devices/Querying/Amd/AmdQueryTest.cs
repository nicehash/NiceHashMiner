using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Devices.Querying.Amd;
using NiceHashMiner.Devices.Querying.Amd.OpenCL;
using System.Collections.Generic;

namespace NiceHashMinerLegacy.Tests.Devices.Querying.Amd
{
    [TestClass]
    public class AmdQueryTest
    {
        private class AmdQueryDummy : AmdQuery
        {
            private class OpenCLQueryDummy : QueryOpenCL
            {
                protected override string GetQueryString()
                {
                    return OclTestData.TestData1;
                }
            }

            private class AdlQueryDummy : QueryAdl
            {
                public override bool TryQuery(out Dictionary<int, AmdBusIDInfo> busIdInfos, out int numDevs)
                {
                    busIdInfos = new Dictionary<int, AmdBusIDInfo>();
                    numDevs = 5;

                    foreach (var busID in OclTestData.TestData1BusIDs)
                    {
                        busIdInfos[busID] = new AmdBusIDInfo("test", "test", "test", -1, -1);
                    }

                    return true;
                }
            }

            public AmdQueryDummy() : base(0)
            {
                OclQuery = new OpenCLQueryDummy();
                AdlQuery = new AdlQueryDummy();
            }
        }

        [TestMethod]
        public void QueryResult_ShouldMatch()
        {
            var query = new AmdQueryDummy();
            query.QueryOpenCLDevices();

            SystemSpecs.QueryVideoControllers();

            var amdDevs = query.QueryAmd(out var failedDriverCheck);

            Assert.AreEqual(5, amdDevs.Count);

            for (var i = 0; i < amdDevs.Count; i++)
            {
                Assert.AreEqual(i, amdDevs[i].ID);
            }

            Assert.AreEqual(4, amdDevs[0].IDByBus);
            Assert.AreEqual(1, amdDevs[1].IDByBus);
            Assert.AreEqual(0, amdDevs[2].IDByBus);
            Assert.AreEqual(3, amdDevs[3].IDByBus);
            Assert.AreEqual(2, amdDevs[4].IDByBus);
        }
    }
}
