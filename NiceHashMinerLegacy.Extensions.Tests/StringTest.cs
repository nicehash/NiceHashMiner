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

            var b = "";
        }
    }
}
