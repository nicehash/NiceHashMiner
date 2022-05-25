using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using NHMCore.Nhmws;
using NHMCore.Nhmws.V3;


namespace NHMCoreTests.Nhmws
{
    [TestClass]
    public class MessageParserTests
    {
        private static T ParseMessageFromFile<T>(string pathRelativeUnitTestingFile) where T : IMethod
        {
            var msg = MessageParserV3.ParseMessage(Paths.LoadTextFile(pathRelativeUnitTestingFile));
            if (msg is T msgTyped) return msgTyped;
            throw new Exception($"Test file '{pathRelativeUnitTestingFile}' could not be parsed as type '{typeof(T).Name}' got '{msg.GetType().Name}'");
        }

        [TestMethod]
        public void TestParse_SMA()
        {
            var sma = ParseMessageFromFile<SmaMessage>(@"Nhmws\data\sma01.json");
            var (payingDict, stables) = MessageParser.ParseSmaMessageData(sma);
            Assert.AreEqual(3, stables.Count(), "Stable count differs");
            Assert.AreEqual(24, payingDict.Count(), "SMA paying count differs");
        }
    }
}
