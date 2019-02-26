using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Devices.Querying.Amd;
using NiceHashMiner.Devices.Querying.Amd.OpenCL;

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

            public AmdQueryDummy() : base(0)
            {
                OclQuery = new OpenCLQueryDummy();
            }
        }


    }
}
