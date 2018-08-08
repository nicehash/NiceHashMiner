using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NiceHashMinerLegacy.Extensions.Tests
{
    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void AfterFirstNumberShouldMatch()
        {
            var a = "NVIDIA GTX 1080 Ti".AfterFirstOccurence("NVIDIA ");
            Assert.AreEqual("GTX 1080 Ti", a);

            var b = "blarg".AfterFirstOccurence("NV ");
            Assert.AreEqual("", b);

            var c = "NVIDIA GTX 750 Ti".AfterFirstOccurence("GTX");
            Assert.AreEqual(" 750 Ti", c);
        }
    }
}
