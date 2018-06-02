using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NiceHashMinerLegacy.Extensions.Tests
{
    [TestClass]
    public class DateTimeTest
    {
        [TestMethod]
        public void GetUnixTime_ShouldMatch()
        {
            const ulong mil = 1520438617332;
            var date = new DateTime(2018, 3, 7, 16, 3, 37, 332);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            Assert.AreEqual(mil, date.GetUnixTime());
        }
    }
}
