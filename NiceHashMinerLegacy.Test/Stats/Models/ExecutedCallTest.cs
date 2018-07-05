using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Stats.Models;

namespace NiceHashMinerLegacy.Tests.Stats.Models
{
    [TestClass]
    public class ExecutedCallTest
    {
        [TestMethod]
        public void ErrorFreeShouldMatch()
        {
            const string expected = "{\"method\":\"executed\",\"params\":[0,0]}";
            var call = new ExecutedCall(0, null).Serialize();
            Assert.AreEqual(expected, call);
        }

        [TestMethod]
        public void ErrorShouldMatch()
        {
            const string expected = "{\"method\":\"executed\",\"params\":[0,1,\"This is an error\"]}";
            var call = new ExecutedCall(1, "This is an error").Serialize();
            Assert.AreEqual(expected, call);
        }
    }
}
