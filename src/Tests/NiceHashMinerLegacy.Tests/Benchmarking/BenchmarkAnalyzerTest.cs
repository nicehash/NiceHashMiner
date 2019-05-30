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
            Assert.AreEqual(exp, ret);

            input = new List<double>() { 1.0, 1.2, 1.7, 2.5, 2.2, 2.7, 2.5 , 2.0, 1.9, 1.8};
            ret = BenchmarkingAnalyzer.CalcAverageSpeed(input);
            exp = 1.95;
            Assert.AreEqual(exp, ret);

        }
        [TestMethod]
        public void NormalizedStandardDeviation()
        {
            var input = new List<double>() { 20.0, 15.0, 10.0 };
            var ret = BenchmarkingAnalyzer.NormalizedStandardDeviation(input);
            var exp = new List<double>() { 0.19245008972987526, 0.0, 0.19245008972987526 };
            CollectionAssert.AreEqual(exp, ret);

            input = new List<double>() { 9.8, 10.0, 10.1, 9.9, 10.0, 9.5};
            ret = BenchmarkingAnalyzer.NormalizedStandardDeviation(input);
            exp = new List<double>() { 0.0034422284187509045, 0.0048191197862513541, 0.0089497938887524824, 0.00068844568375022483, 0.0048191197862513541, 0.015834250726254363 };
            CollectionAssert.AreEqual(exp, ret);
        }

        [TestMethod]
        public void UpdateMiningSpeeds()
        {
            //here we test Mining Speeds List with cap size (120)
            for (int i = 0; i < 120; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 1, SecondarySpeed = 0 });
            }
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 2, SecondarySpeed = 2 });

            var exp = new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 2, SecondarySpeed = 2 };

            Assert.IsTrue(BenchmarkingAnalyzer.MiningSpeeds["test"].Count == 120);
            Assert.AreEqual(exp, BenchmarkingAnalyzer.MiningSpeeds["test"][119]);
        }

        [TestMethod]
        public void CheckIfDeviant()
        {
            var input = new List<double>() { 20.0, 15.0, 10.0 };
            var ret = BenchmarkingAnalyzer.CheckIfDeviant(input);
            var exp = false;
            Assert.AreEqual(exp, ret);

            input = new List<double>() { 9.8, 10.0, 10.1, 9.9, 10.0, 9.5 };
            ret = BenchmarkingAnalyzer.CheckIfDeviant(input);
            exp = true;
            Assert.AreEqual(exp, ret);

            input = new List<double>() { 9.8, 10.0, 10.1, 9.9, 10.0, 9.5 , 20};
            ret = BenchmarkingAnalyzer.CheckIfDeviant(input);
            exp = false;
            Assert.AreEqual(exp, ret);
        }

        [TestMethod]
        public void IsDeviant()
        {
            //only primary speeds
            for (int i = 0; i < 10; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 10.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now + TimeSpan.FromSeconds(i * 5) });
            }
            var ret = BenchmarkingAnalyzer.IsDeviant("test");
            var exp = true;
            Assert.AreEqual(exp, ret);

            BenchmarkingAnalyzer.MiningSpeeds.Clear();

            //we add secondary speed
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 10.0, SecondarySpeed = 5.0, Timestamp = DateTime.Now + TimeSpan.FromSeconds(i * 5) });
            }
            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = true;
            Assert.AreEqual(exp, ret);

            BenchmarkingAnalyzer.MiningSpeeds.Clear();

            //secondary speed is not deviant
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 10.0, SecondarySpeed = 5.0, Timestamp = DateTime.Now + TimeSpan.FromSeconds(i * 5) });
            }
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 10.0, SecondarySpeed = 20.0, Timestamp = DateTime.Now + TimeSpan.FromSeconds(i * 5) });
            }
            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = false;
            Assert.AreEqual(exp, ret);

            //clear state
            BenchmarkingAnalyzer.MiningSpeeds.Clear();

            //primary speed is not deviant
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 15.0, SecondarySpeed = 10.0, Timestamp = DateTime.Now + TimeSpan.FromSeconds(i * 5) });
            }
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 9.0, SecondarySpeed = 10.0, Timestamp = DateTime.Now + TimeSpan.FromSeconds(i * 5) });
            }
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 30.0, SecondarySpeed = 10.0, Timestamp = DateTime.Now});

            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = false;
            Assert.AreEqual(exp, ret);

            BenchmarkingAnalyzer.MiningSpeeds.Clear();

            //old benchmarks
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 15.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now.AddHours(-2) });
            }
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 30.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now + TimeSpan.FromSeconds(i * 5) });
            }
            for (int i = 0; i < 5; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 31.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now + TimeSpan.FromSeconds(i * 5) });
            }

            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = false;
            Assert.AreEqual(exp, ret);

            BenchmarkingAnalyzer.MiningSpeeds.Clear();

            //not enough speeds
            for (int i = 0; i < 3; i++)
            {
                BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 15.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now });
            }
            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = false;
            Assert.AreEqual(exp, ret);

            BenchmarkingAnalyzer.MiningSpeeds.Clear();

            //old speeds but close enough
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 30.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now.AddHours(-2) });
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 30.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now.AddHours(-2).AddSeconds(15) });
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 30.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now.AddHours(-2).AddSeconds(50) });
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 30.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now.AddHours(-2).AddMinutes(1) });
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 30.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now.AddHours(-2).AddMinutes(1).AddSeconds(15) });
            BenchmarkingAnalyzer.UpdateMiningSpeeds("test", new BenchmarkingAnalyzer.MiningSpeed { PrimarySpeed = 30.0, SecondarySpeed = 0.0, Timestamp = DateTime.Now.AddHours(-2).AddMinutes(3) });
            ret = BenchmarkingAnalyzer.IsDeviant("test");
            exp = true;
            Assert.AreEqual(exp, ret);
        }
    }
}
