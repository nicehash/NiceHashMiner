using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Devices;

namespace NiceHashMinerLegacy.Tests.Devices.Algorithms
{
    [TestClass]
    public class DefaultAlgorithmsTest
    {
        [TestMethod]
        public void XmrStakAlgorithmsForDevice_Tests()
        {
            var cpuDevice = new CpuComputeDevice(0, "grp", "intel i7-8700k", 16, 0, 0);
            var algorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cpuDevice);

            Assert.AreEqual(3, algorithms.Count);
             
            foreach (var algorithm in algorithms) //check miner base type
            {
                Assert.AreEqual(MinerBaseType.XmrStak, algorithm.MinerBaseType);
            }

            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.CryptoNightHeavy,
                AlgorithmType.CryptoNightV8,
                AlgorithmType.CryptoNightR,
            };
            foreach (var secondaryAlgorithm in secondaryList) //check to see if all algorithms from secondary list are in algorithms
            {
                var algo = new Algorithm(MinerBaseType.XmrStak, secondaryAlgorithm);
                Assert.IsTrue(algorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
        }

        [TestMethod]
        public void ClaymoreDualAlgorithmsForDevice_Tests()
        {
            var device = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(device);
            List<Algorithm> claymoreAlgorithms = new List<Algorithm>();
            foreach(var algorithm in allAlgorithms)
            {
                if(algorithm.MinerBaseType == MinerBaseType.Claymore)
                {
                    claymoreAlgorithms.Add(algorithm);
                }
            }       
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.NONE,
                AlgorithmType.Decred,
                AlgorithmType.Blake2s,
                AlgorithmType.Keccak,
            };
            foreach (var secondaryAlgorithm in secondaryList) //check to see if all algorithms from secondary list are in algorithms (dual with dager)
            {
                var algo = new Algorithm(MinerBaseType.Claymore, secondaryAlgorithm);
                Assert.IsTrue(claymoreAlgorithms.Any(a => algo.NiceHashID == a.SecondaryNiceHashID));
            }

            foreach (var algorithm in claymoreAlgorithms)
            {
                if (algorithm.SecondaryNiceHashID == AlgorithmType.NONE) //check that all duals are disabled by default
                {
                    Assert.IsTrue(algorithm.Enabled);
                }  else
                {
                    Assert.IsFalse(algorithm.Enabled);
                }
            }

            claymoreAlgorithms.Clear();
            device = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(device);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.Claymore)
                {
                    claymoreAlgorithms.Add(algorithm);
                }
            }

            Assert.AreEqual(0, claymoreAlgorithms.Count);
        }

        [TestMethod]
        public void PhoenixAlgorithmsForDevice_Tests()
        {
            var device = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(device);
            List<Algorithm> phoenixAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.Phoenix)
                {
                    phoenixAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.DaggerHashimoto,
            };
            foreach (var secondaryAlgorithm in secondaryList) //check to see if all algorithms from secondary list are in algorithms (dual with dager)
            {
                var algo = new Algorithm(MinerBaseType.Phoenix, secondaryAlgorithm);
                Assert.IsTrue(phoenixAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }

            phoenixAlgorithms.Clear();
            device = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(device);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.Phoenix)
                {
                    phoenixAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, phoenixAlgorithms.Count);
        }

        [TestMethod]
        public void GMinerAlgorithmsForDevice_Tests()
        {
            var cudaDevice = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDevice);
            List<Algorithm> gminerAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.GMiner)
                {
                    gminerAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.ZHash,
                AlgorithmType.Beam,
                AlgorithmType.GrinCuckaroo29,
            };
            foreach (var secondaryAlgorithm in secondaryList) //check to see if all algorithms from secondary list are in algorithms (dual with dager)
            {
                var algo = new Algorithm(MinerBaseType.GMiner, secondaryAlgorithm);
                Assert.IsTrue(gminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }

            gminerAlgorithms.Clear();
            cudaDevice = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.GMiner)
                {
                    gminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, gminerAlgorithms.Count);

            gminerAlgorithms.Clear(); //amd vega device
            var openCLDevice = amdComputeDev();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(openCLDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.GMiner)
                {
                    gminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(1, gminerAlgorithms.Count);
            Assert.AreEqual(AlgorithmType.Beam, gminerAlgorithms[0].NiceHashID);

            gminerAlgorithms.Clear(); //amd GCN too low
            openCLDevice = amdComputeDevLowGCN();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(openCLDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.GMiner)
                {
                    gminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, gminerAlgorithms.Count);
        }

        [TestMethod]
        public void sgminerAlgorithmsForDevice_tests()
        {
            var openCLDevice = amdComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(openCLDevice);
            List<Algorithm> sgminerAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.sgminer)
                {
                    sgminerAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.NeoScrypt,
                AlgorithmType.Pascal,
                AlgorithmType.Keccak,
                AlgorithmType.DaggerHashimoto,
                AlgorithmType.X16R,
            };
            foreach (var secondaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.sgminer, secondaryAlgorithm);
                Assert.IsTrue(sgminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(5, sgminerAlgorithms.Count);

            sgminerAlgorithms.Clear(); //amd gpu ram too low
            openCLDevice = amdComputeDevLowRAM();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(openCLDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.sgminer)
                {
                    sgminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(4, sgminerAlgorithms.Count);

            sgminerAlgorithms.Clear(); //bad amd drivers
            openCLDevice = amdComputeDevDriverDisabled();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(openCLDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.sgminer)
                {
                    sgminerAlgorithms.Add(algorithm);
                }
            }
            secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.Pascal,
                AlgorithmType.Keccak,
                AlgorithmType.DaggerHashimoto,
                AlgorithmType.X16R,
            };
            foreach (var secondaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.sgminer, secondaryAlgorithm);
                Assert.IsTrue(sgminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(4, sgminerAlgorithms.Count);

            sgminerAlgorithms.Clear(); //cuda device
            var cudaDevice = cudaComputeDev();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.sgminer)
                {
                    sgminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, sgminerAlgorithms.Count);
        }

        [TestMethod]
        public void ProspectorAlgorithmsForDevice_tests()
        {
            var openCLDevice = amdComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(openCLDevice);
            List<Algorithm> prospectorAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.Prospector)
                {
                    prospectorAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.Skunk,
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.Prospector, primaryAlgorithm);
                Assert.IsTrue(prospectorAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(1, prospectorAlgorithms.Count);
            Assert.IsFalse(prospectorAlgorithms[0].Enabled);

            prospectorAlgorithms.Clear(); //cuda device
            var cudaDevice = cudaComputeDev();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.Prospector)
                {
                    prospectorAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, prospectorAlgorithms.Count);
        }

        [TestMethod]
        public void ccminerAlgorithmsForDevice_tests()
        {
            var cudaDev = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            List<Algorithm> ccminerAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.ccminer || algorithm.MinerBaseType == MinerBaseType.ccminer_alexis)
                {
                    ccminerAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.NeoScrypt,
                AlgorithmType.Blake2s,
                AlgorithmType.Keccak,
                AlgorithmType.Skunk,
                AlgorithmType.X16R,
                AlgorithmType.Lyra2REv3,
                AlgorithmType.MTP,
                AlgorithmType.Keccak, //ccminer_alexis
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.ccminer, primaryAlgorithm);
                Assert.IsTrue(ccminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(8, ccminerAlgorithms.Count);

            ccminerAlgorithms.Clear(); //major number = 3
            cudaDev = cudaComputeDev3Major();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.ccminer || algorithm.MinerBaseType == MinerBaseType.ccminer_alexis)
                {
                    ccminerAlgorithms.Add(algorithm);
                }
            }
            secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.Blake2s,
                AlgorithmType.Keccak,
                AlgorithmType.Skunk,
                AlgorithmType.X16R,
                AlgorithmType.MTP,
                AlgorithmType.Keccak, //ccminer_alexis
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.ccminer, primaryAlgorithm);
                Assert.IsTrue(ccminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(6, ccminerAlgorithms.Count);

            ccminerAlgorithms.Clear(); //low major
            cudaDev = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.ccminer || algorithm.MinerBaseType == MinerBaseType.ccminer_alexis)
                {
                    ccminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, ccminerAlgorithms.Count);
        }

        [TestMethod]
        public void ethminerAlgorithmsForDevice_tests()
        {
            var cudaDev = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            List<Algorithm> ethminerAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.ethminer)
                {
                    ethminerAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.DaggerHashimoto,
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.ethminer, primaryAlgorithm);
                Assert.IsTrue(ethminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(1, ethminerAlgorithms.Count);
            Assert.IsFalse(ethminerAlgorithms[0].Enabled);

            ethminerAlgorithms.Clear(); //low major
            cudaDev = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.ethminer)
                {
                    ethminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, ethminerAlgorithms.Count);

            ethminerAlgorithms.Clear(); //750 ti
            cudaDev = cudaComputeDev750Ti();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.ethminer)
                {
                    ethminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, ethminerAlgorithms.Count);
        }

        [TestMethod]
        public void EWBFAlgorithmsForDevice_tests()
        {
            var cudaDev = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            List<Algorithm> ewbfAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.EWBF)
                {
                    ewbfAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.ZHash,
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.EWBF, primaryAlgorithm);
                Assert.IsTrue(ewbfAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(1, ewbfAlgorithms.Count);
            Assert.IsTrue(ewbfAlgorithms[0].Enabled);

            ewbfAlgorithms.Clear(); //low major
            cudaDev = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.EWBF)
                {
                    ewbfAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, ewbfAlgorithms.Count);
        }

        [TestMethod]
        public void trexAlgorithmsForDevice_tests()
        {
            var cudaDev = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            List<Algorithm> trexAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.trex)
                {
                    trexAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.Skunk,
                AlgorithmType.X16R,
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.trex, primaryAlgorithm);
                Assert.IsTrue(trexAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(2, trexAlgorithms.Count);

            trexAlgorithms.Clear(); //low major
            cudaDev = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.trex)
                {
                    trexAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, trexAlgorithms.Count);
        }

        [TestMethod]
        public void BMinerAlgorithmsForDevice_tests()
        {
            var cudaDev = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            List<Algorithm> bminerAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.BMiner)
                {
                    bminerAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.ZHash,
                AlgorithmType.DaggerHashimoto,
                AlgorithmType.Beam,
                AlgorithmType.GrinCuckaroo29,
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.BMiner, primaryAlgorithm);
                Assert.IsTrue(bminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(4, bminerAlgorithms.Count);
            foreach(var algo in bminerAlgorithms)
            {
                Assert.IsFalse(algo.Enabled);
            }

            bminerAlgorithms.Clear(); //low ram
            cudaDev = cudaComputeDevLowRam();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.BMiner)
                {
                    bminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(1, bminerAlgorithms.Count);
            Assert.AreEqual(AlgorithmType.ZHash, bminerAlgorithms[0].NiceHashID);

            bminerAlgorithms.Clear(); //low major
            cudaDev = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.BMiner)
                {
                    bminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, bminerAlgorithms.Count);

            bminerAlgorithms.Clear(); //amd device
            var openCLDevice = amdComputeDev();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(openCLDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.BMiner)
                {
                    bminerAlgorithms.Add(algorithm);
                }
            }
            secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.Beam,
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.BMiner, primaryAlgorithm);
                Assert.IsTrue(bminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(1, bminerAlgorithms.Count);
            Assert.AreEqual(AlgorithmType.Beam, bminerAlgorithms[0].NiceHashID);

            bminerAlgorithms.Clear(); //amd low gcn
            openCLDevice = amdComputeDevLowGCN();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(openCLDevice);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.BMiner)
                {
                    bminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, bminerAlgorithms.Count);
        }

        [TestMethod]
        public void TTMinerAlgorithmsForDevice_tests()
        {
            var cudaDev = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            List<Algorithm> ttminerAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.TTMiner)
                {
                    ttminerAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.MTP,
                AlgorithmType.Lyra2REv3,
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.TTMiner, primaryAlgorithm);
                Assert.IsTrue(ttminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(2, ttminerAlgorithms.Count);
            foreach(var algo in ttminerAlgorithms)
            {
                Assert.IsFalse(algo.Enabled);
            }

            ttminerAlgorithms.Clear(); //low major
            cudaDev = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.TTMiner)
                {
                    ttminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, ttminerAlgorithms.Count);
        }

        [TestMethod]
        public void NBMinerAlgorithmsForDevice_tests()
        {
            var cudaDev = cudaComputeDev();
            var allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            List<Algorithm> nbminerAlgorithms = new List<Algorithm>();
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.NBMiner)
                {
                    nbminerAlgorithms.Add(algorithm);
                }
            }
            var secondaryList = new List<AlgorithmType>
            {
                AlgorithmType.GrinCuckaroo29,
                AlgorithmType.GrinCuckatoo31,
            };
            foreach (var primaryAlgorithm in secondaryList)
            {
                var algo = new Algorithm(MinerBaseType.NBMiner, primaryAlgorithm);
                Assert.IsTrue(nbminerAlgorithms.Any(a => algo.NiceHashID == a.NiceHashID));
            }
            Assert.AreEqual(2, nbminerAlgorithms.Count);
            foreach (var algo in nbminerAlgorithms)
            {
                Assert.IsTrue(algo.Enabled);
            }

            nbminerAlgorithms.Clear(); //low major
            cudaDev = cudaComputeDevLowMajor();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.NBMiner)
                {
                    nbminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, nbminerAlgorithms.Count);

            nbminerAlgorithms.Clear(); //low ram
            cudaDev = cudaComputeDevLowRam();
            allAlgorithms = DefaultAlgorithms.GetAlgorithmsForDevice(cudaDev);
            foreach (var algorithm in allAlgorithms)
            {
                if (algorithm.MinerBaseType == MinerBaseType.NBMiner)
                {
                    nbminerAlgorithms.Add(algorithm);
                }
            }
            Assert.AreEqual(0, nbminerAlgorithms.Count);
        }

        private CudaComputeDevice cudaComputeDev()
        {
            var cudaDev = new CudaDevice();
            cudaDev.DeviceID = 0;
            cudaDev.pciBusID = 1;
            cudaDev.VendorName = "MSI";
            cudaDev.DeviceName = "GeForce GTX 1080 Ti";
            cudaDev.SM_major = 6;
            cudaDev.SM_minor = 1;
            cudaDev.UUID = "cudaDeviceUUID";
            cudaDev.DeviceGlobalMemory = 11589934592;
            var cudaComputeDev = new CudaComputeDevice(cudaDev, DeviceGroupType.NVIDIA_6_x, 0, new NVIDIA.NVAPI.NvPhysicalGpuHandle(), new ManagedCuda.Nvml.nvmlDevice());
            return cudaComputeDev;
        }

        private CudaComputeDevice cudaComputeDevLowRam()
        {
            var cudaDev = new CudaDevice();
            cudaDev.DeviceID = 0;
            cudaDev.pciBusID = 1;
            cudaDev.VendorName = "MSI";
            cudaDev.DeviceName = "GeForce GTX 980";
            cudaDev.SM_major = 6;
            cudaDev.SM_minor = 1;
            cudaDev.UUID = "cudaDeviceUUID";
            cudaDev.DeviceGlobalMemory = 2221225472;
            var cudaComputeDev = new CudaComputeDevice(cudaDev, DeviceGroupType.NVIDIA_6_x, 0, new NVIDIA.NVAPI.NvPhysicalGpuHandle(), new ManagedCuda.Nvml.nvmlDevice());
            return cudaComputeDev;
        }

        private CudaComputeDevice cudaComputeDev3Major()
        {
            var cudaDev = new CudaDevice();
            cudaDev.DeviceID = 0;
            cudaDev.pciBusID = 1;
            cudaDev.VendorName = "MSI";
            cudaDev.DeviceName = "GeForce GTX 980 Ti";
            cudaDev.SM_major = 3;
            cudaDev.SM_minor = 1;
            cudaDev.UUID = "cudaDeviceUUID";
            cudaDev.DeviceGlobalMemory = 5589934592;
            var cudaComputeDev = new CudaComputeDevice(cudaDev, DeviceGroupType.NVIDIA_3_x, 0, new NVIDIA.NVAPI.NvPhysicalGpuHandle(), new ManagedCuda.Nvml.nvmlDevice());
            return cudaComputeDev;
        }

        private CudaComputeDevice cudaComputeDev750Ti()
        {
            var cudaDev = new CudaDevice();
            cudaDev.DeviceID = 0;
            cudaDev.pciBusID = 1;
            cudaDev.VendorName = "MSI";
            cudaDev.DeviceName = "GeForce GTX 750 Ti";
            cudaDev.SM_major = 5;
            cudaDev.SM_minor = 1;
            cudaDev.UUID = "cudaDeviceUUID";
            cudaDev.DeviceGlobalMemory = 5589934592;
            var cudaComputeDev = new CudaComputeDevice(cudaDev, DeviceGroupType.NVIDIA_5_x, 0, new NVIDIA.NVAPI.NvPhysicalGpuHandle(), new ManagedCuda.Nvml.nvmlDevice());
            return cudaComputeDev;
        }

        private CudaComputeDevice cudaComputeDevLowMajor()
        {
            var cudaDev = new CudaDevice();
            cudaDev.DeviceID = 0;
            cudaDev.pciBusID = 1;
            cudaDev.VendorName = "MSI";
            cudaDev.DeviceName = "GeForce GTX 540";
            cudaDev.SM_major = 2;
            cudaDev.SM_minor = 1;
            cudaDev.UUID = "cudaDeviceUUID";
            cudaDev.DeviceGlobalMemory = 11589934592;
            var cudaComputeDev = new CudaComputeDevice(cudaDev, DeviceGroupType.NVIDIA_2_1, 0, new NVIDIA.NVAPI.NvPhysicalGpuHandle(), new ManagedCuda.Nvml.nvmlDevice());
            return cudaComputeDev;
        }

        private AmdComputeDevice amdComputeDev()
        {
            var openCLDev = new NiceHashMiner.Devices.Querying.Amd.OpenCL.OpenCLDevice();
            openCLDev.AMD_BUS_ID = 0;
            openCLDev.DeviceID = 0;
            openCLDev._CL_DEVICE_GLOBAL_MEM_SIZE = 11589934592;
            openCLDev._CL_DEVICE_NAME = "Vega 64";
            var amdDev = new AmdGpuDevice(openCLDev,"", false, "Vega 64", "vegaGpuUUID");
            var amdCompDev = new AmdComputeDevice(amdDev, 0, false, 0);
            return amdCompDev;
        }

        private AmdComputeDevice amdComputeDevLowRAM()
        {
            var openCLDev = new NiceHashMiner.Devices.Querying.Amd.OpenCL.OpenCLDevice();
            openCLDev.AMD_BUS_ID = 0;
            openCLDev.DeviceID = 0;
            openCLDev._CL_DEVICE_GLOBAL_MEM_SIZE = 2589934592;
            openCLDev._CL_DEVICE_NAME = "Vega 64";
            var amdDev = new AmdGpuDevice(openCLDev, "", false, "Vega 64", "vegaGpuUUID");
            var amdCompDev = new AmdComputeDevice(amdDev, 0, false, 0);
            return amdCompDev;
        }

        private AmdComputeDevice amdComputeDevLowGCN()
        {
            var openCLDev = new NiceHashMiner.Devices.Querying.Amd.OpenCL.OpenCLDevice();
            openCLDev.AMD_BUS_ID = 0;
            openCLDev.DeviceID = 0;
            openCLDev._CL_DEVICE_GLOBAL_MEM_SIZE = 11589934592;
            openCLDev._CL_DEVICE_NAME = "Radeon R7 360";
            var amdDev = new AmdGpuDevice(openCLDev, "", false, "R7 360", "radeonGpuUUID");
            var amdCompDev = new AmdComputeDevice(amdDev, 0, false, 0);
            return amdCompDev;
        }

        private AmdComputeDevice amdComputeDevDriverDisabled()
        {
            var openCLDev = new NiceHashMiner.Devices.Querying.Amd.OpenCL.OpenCLDevice();
            openCLDev.AMD_BUS_ID = 0;
            openCLDev.DeviceID = 0;
            openCLDev._CL_DEVICE_GLOBAL_MEM_SIZE = 11589934592;
            openCLDev._CL_DEVICE_NAME = "Vega 64";
            var amdDev = new AmdGpuDevice(openCLDev, "", true, "Vega 64", "radeonGpuUUID");
            var amdCompDev = new AmdComputeDevice(amdDev, 0, false, 0);
            return amdCompDev;
        }
    }
}
