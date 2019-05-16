using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Benchmarking;
using System;
using System.Collections.Generic;

namespace NiceHashMinerLegacy.Tests.Benchmarking
{
    [TestClass]
    public class BenchmarkAnalyzerTest
    {
        [TestMethod]
        public void CalcAverageSpeed()
        {
            var input = new List<double>() { 0.0, 5.0, 2.5};
            var ret = BenchmarkingAnalyzer.CalcAverageSpeed(input);
            var exp = 2.5;
            Assert.AreEqual(ret, exp);

            input = new List<double>() { 1.0, 1.2, 1.7, 2.5, 2.2, 2.7, 2.5 , 2.0, 1.9, 1.8};
            ret = BenchmarkingAnalyzer.CalcAverageSpeed(input);
            exp = 1.95;
            Assert.AreEqual(ret, exp);

        }
        [TestMethod]
        public void NormalizedStandardDeviation()
        {
            var input = new List<double>() { 20.0, 15.0, 10.0 };
            var ret = BenchmarkingAnalyzer.NormalizedStandardDeviation(input);
            var exp = new List<double>() { 0.19245008972987526, 0.0, 0.19245008972987526 };
            CollectionAssert.AreEqual(ret, exp);

            input = new List<double>() { 9.8, 10.0, 10.1, 9.9, 10.0, 9.5};
            ret = BenchmarkingAnalyzer.NormalizedStandardDeviation(input);
            exp = new List<double>() { 0.0034422284187509045, 0.0048191197862513541, 0.0089497938887524824, 0.00068844568375022483, 0.0048191197862513541, 0.015834250726254363 };
            CollectionAssert.AreEqual(ret, exp);
        }

        [TestMethod]
        public void UpdateMiningSpeeds()
        {
            //here we test Mining Speeds List with cap size (120)
            for (int i = 0; i < 120; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 1, secondarySpeed = 0 });
            }
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 2, secondarySpeed = 2 });

            var exp = new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 2, secondarySpeed = 2 };

            Assert.IsTrue(BenchmarkingAnalyzer.MiningSpeeds["test"].Count == 120);
            Assert.AreEqual(BenchmarkingAnalyzer.MiningSpeeds["test"][119], exp);
        }

        [TestMethod]
        public void CheckIfDeviant()
        {
            var input = new List<double>() { 20.0, 15.0, 10.0 };
            var ret = BenchmarkingAnalyzer.CheckIfDeviant(input);
            var exp = false;
            Assert.AreEqual(ret, exp);

            input = new List<double>() { 9.8, 10.0, 10.1, 9.9, 10.0, 9.5 };
            ret = BenchmarkingAnalyzer.CheckIfDeviant(input);
            exp = true;
            Assert.AreEqual(ret, exp);

            input = new List<double>() { 9.8, 10.0, 10.1, 9.9, 10.0, 9.5 , 20};
            ret = BenchmarkingAnalyzer.CheckIfDeviant(input);
            exp = false;
            Assert.AreEqual(ret, exp);
        }

        [TestMethod]
        public void IsDeviant()
        {
            //only primary speeds
            for (int i = 0; i < 10; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 10.0, secondarySpeed = 0.0, time = DateTime.Now });
            }
            var ret = BenchmarkingAnalyzer.IsDeviant("test");
            var exp = true;
            Assert.AreEqual(ret, exp);

            //we add secondary speed
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 10.0, secondarySpeed = 5.0, time = DateTime.Now });
            }
            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = true;
            Assert.AreEqual(ret, exp);

            //secondary speed is not deviant
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 10.0, secondarySpeed = 5.0, time = DateTime.Now });
            }
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 10.0, secondarySpeed = 20.0, time = DateTime.Now });
            }
            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = false;
            Assert.AreEqual(ret, exp);

            //clear state
            BenchmarkingAnalyzer.MiningSpeeds.Clear();

            //primary speed is not deviant
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 15.0, secondarySpeed = 10.0, time = DateTime.Now });
            }
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 9.0, secondarySpeed = 10.0, time = DateTime.Now });
            }
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 30.0, secondarySpeed = 10.0, time = DateTime.Now });

            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = false;
            Assert.AreEqual(ret, exp);

            //old benchmarks
            BenchmarkingAnalyzer.MiningSpeeds.Clear();
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 15.0, secondarySpeed = 0.0, time = DateTime.Now.AddHours(-2) });
            }
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 30.0, secondarySpeed = 0.0, time = DateTime.Now });
            }
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { primarySpeed = 31.0, secondarySpeed = 0.0, time = DateTime.Now });
            }

            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = true;
            Assert.AreEqual(ret, exp);
        }
    }
}
