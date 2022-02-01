using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static NHM.CredentialValidators.CredentialValidators;

namespace NHM.CredentialValidatorsTests
{
    [TestClass]
    public class CredentialValidatorsTests
    {
        internal class TestLabel
        {
            private int _testCount = 0;
            public string label() => $"#{++_testCount}";
        }

        [TestMethod]
        public void TestValidateWorkerName()
        {
            Assert.AreEqual(true, ValidateWorkerName("worker1"));
            Assert.AreEqual(false, ValidateWorkerName("wor ker1"), "Whitespace");
            Assert.AreEqual(false, ValidateWorkerName("worker12345678910123456789"), "too long");
        }

        [TestMethod]
        public void TestValidateBitcoinAddressBase58()
        {
            var tl = new TestLabel { };
            Func<string, bool, bool> isValid = ValidateBitcoinAddressBase58;
            Assert.AreEqual(true,  isValid("17VZNX1SN5NtKa8UQFxwQbFeFc3iqRYhem", true), tl.label());
            Assert.AreEqual(true,  isValid("33hGFJZQAfbdzyHGqhJPvZwncDjUBdZqjW", true), tl.label());
            Assert.AreEqual(false, isValid("2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS", true), tl.label());
            Assert.AreEqual(true,  isValid("2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS", false), tl.label());
            Assert.AreEqual(false, isValid("whatever", false), tl.label());
            Assert.AreEqual(false, isValid("", false), tl.label());
            Assert.AreEqual(false, isValid(" ", false), tl.label());
            Assert.AreEqual(false, isValid(null, false), tl.label());
            Assert.AreEqual(false, isValid("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", true), tl.label());
            Assert.AreEqual(false, isValid("tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsx", false), tl.label());
            Assert.AreEqual(false, isValid("tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsc", false), tl.label());
            Assert.AreEqual(false, isValid("tc1qw508d6qejxtdg4y5r3zarvary0c5xw7kg3g4ty", false), tl.label());
            Assert.AreEqual(false, isValid("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t5", true), tl.label());
        }

        [TestMethod]
        public void TestValidateBitcoinAddressBech32()
        {
            var tl = new TestLabel { };
            Func<string, bool, bool> isValid = ValidateBitcoinAddressBech32;
            Assert.AreEqual(false, isValid("17VZNX1SN5NtKa8UQFxwQbFeFc3iqRYhem", true), tl.label());
            Assert.AreEqual(false, isValid("33hGFJZQAfbdzyHGqhJPvZwncDjUBdZqjW", true), tl.label());
            Assert.AreEqual(false, isValid("2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS", true), tl.label());
            Assert.AreEqual(false, isValid("2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS", false), tl.label());
            Assert.AreEqual(false, isValid("whatever", false), tl.label());
            Assert.AreEqual(false, isValid("", false), tl.label());
            Assert.AreEqual(false, isValid(" ", false), tl.label());
            Assert.AreEqual(false, isValid(null, false), tl.label());
            Assert.AreEqual(true, isValid("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", true), tl.label());
            Assert.AreEqual(true, isValid("tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsx", false), tl.label());
            Assert.AreEqual(false, isValid("tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsc", false), tl.label());
            Assert.AreEqual(false, isValid("tc1qw508d6qejxtdg4y5r3zarvary0c5xw7kg3g4ty", false), tl.label());
            Assert.AreEqual(false, isValid("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t5", true), tl.label());
        }

        [TestMethod]
        public void TestValidateBitcoinAddress()
        {
            var tl = new TestLabel{ };
            Func<string, bool, bool> isValid = ValidateBitcoinAddress;
            Assert.AreEqual(true,  isValid("33hGFJZQAfbdzyHGqhJPvZwncDjUBdZqjW", true), tl.label());
            Assert.AreEqual(false, isValid("2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS", true), tl.label());
            Assert.AreEqual(true,  isValid("2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS", false), tl.label());
            Assert.AreEqual(true,  isValid("whatever", false), tl.label());
            Assert.AreEqual(false, isValid("", false), tl.label());
            Assert.AreEqual(false, isValid(" ", false), tl.label());
            Assert.AreEqual(false, isValid(null, false), tl.label());
            Assert.AreEqual(true, isValid("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", true), tl.label());
            Assert.AreEqual(true, isValid("tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsx", false), tl.label());
            Assert.AreEqual(true, isValid("tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsc", false), tl.label());
            Assert.AreEqual(true, isValid("tc1qw508d6qejxtdg4y5r3zarvary0c5xw7kg3g4ty", false), tl.label());
            Assert.AreEqual(false, isValid("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t5", true), tl.label());
        }

    }
}
