using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHM.DeviceDetection.OpenCL.Models;
using NHM.DeviceDetection.AMD;
using NHM.DeviceDetection.WMI;
using System.Linq;
using NHM.Common.Device;

namespace NHM.DeviceDetectionTests
{
    [TestClass]
    public class AMDDetectionTests
    {
        internal class TestLabel
        {
            private int _testCount = 0;
            public string label() => $"#{++_testCount}";
        }
        [TestMethod]
        public void TestParseAMDDevicesToList_DuplicateDevice()
        {
            var tl = new TestLabel { };
            List<VideoControllerData> videoControllerDatas = new List<VideoControllerData>()
            {
                new VideoControllerData("AMD Radeon RX 5700 XT", "AMD Radeon RX 5700 XT", "PCI\\VEN_1002&DEV_731F&SUBSYS_04E21043&REV_C1\\8&3207BE53&0&00000018020B", "30.0.15021.7000", "OK", "ati2mtag_Navi10", 4293918720){PCI_BUS_ID = 8},
                new VideoControllerData("Radeon RX550/550 Series", "Radeon RX550/550 Series", "PCI\\VEN_1002&DEV_699F&SUBSYS_8A901462&REV_C7\\4&1C3D25BB&0&0019", "30.0.15021.7000", "OK", "ati2mtag_Polaris12", 4293918720){PCI_BUS_ID = 12}
            };
            var rawDetection = "{\"ADLErrorString\":\"\",\"AMDBusIDVersionPairs\":[{\"ADLRetCode\":0,\"AdrenalinVersion\":\"22.4.2\",\"BUS_ID\":8,\"FunctionCall\":3},{\"ADLRetCode\":0,\"AdrenalinVersion\":\"22.4.2\",\"BUS_ID\":12,\"FunctionCall\":3}],\"ErrorString\":\"\",\"Platforms\":[{\"Devices\":[{\"BUS_ID\":12,\"DeviceID\":0,\"_CL_DEVICE_BOARD_NAME_AMD\":\"Radeon RX550/ 550 Series\",\"_CL_DEVICE_GLOBAL_MEM_SIZE\":4294967296,\"_CL_DEVICE_NAME\":\"gfx803\",\"_CL_DEVICE_TYPE\":\"GPU\",\"_CL_DEVICE_VENDOR\":\"Advanced Micro Devices, Inc.\",\"_CL_DEVICE_VERSION\":\"OpenCL 2.0 AMD - APP(3380.6)\",\"_CL_DRIVER_VERSION\":\"3380.6(PAL, HSAIL)\"},{\"BUS_ID\":12,\"DeviceID\":1,\"_CL_DEVICE_BOARD_NAME_AMD\":\"Radeon RX550/ 550 Series\",\"_CL_DEVICE_GLOBAL_MEM_SIZE\":4294967296,\"_CL_DEVICE_NAME\":\"gfx803\",\"_CL_DEVICE_TYPE\":\"GPU\",\"_CL_DEVICE_VENDOR\":\"Advanced Micro Devices, Inc.\",\"_CL_DEVICE_VERSION\":\"OpenCL 2.0 AMD - APP(3380.6)\",\"_CL_DRIVER_VERSION\":\"3380.6(PAL, HSAIL)\"},{\"BUS_ID\":8,\"DeviceID\":2,\"_CL_DEVICE_BOARD_NAME_AMD\":\"AMD Radeon RX 5700 XT\",\"_CL_DEVICE_GLOBAL_MEM_SIZE\":8573157376,\"_CL_DEVICE_NAME\":\"gfx1010: xnack - \",\"_CL_DEVICE_TYPE\":\"GPU\",\"_CL_DEVICE_VENDOR\":\"Advanced Micro Devices, Inc.\",\"_CL_DEVICE_VERSION\":\"OpenCL 2.0 AMD - APP(3380.6)\",\"_CL_DRIVER_VERSION\":\"3380.6(PAL, LC)\"}],\"PlatformName\":\"AMD Accelerated Parallel Processing\",\"PlatformNum\":0,\"PlatformVendor\":\"Advanced Micro Devices, Inc.\"}],\"Status\":\"OK\"}";
            var detectionObject = JsonConvert.DeserializeObject<OpenCLDeviceDetectionResult>(rawDetection);
            var result = AMDDetector.ConvertOpenCLResultToList(videoControllerDatas, (rawDetection, detectionObject));
            Assert.IsFalse(result.success, tl.label());
            var result2 = AMDDetector.ConvertOpenCLResultToListFallback(videoControllerDatas, (rawDetection, detectionObject), (rawDetection, detectionObject));
            Assert.AreEqual(2, result2.Count, tl.label());
            Assert.IsTrue(result2.Select(dev => dev.PCIeBusID).Distinct().Count() == 2, tl.label());
        }
        [TestMethod]
        public void TestParseAMDDevicesToList_NoAMD()
        {
            var tl = new TestLabel { };
            List<VideoControllerData> videoControllerDatas = new List<VideoControllerData>()
            {
                new VideoControllerData("NVIDIA GeForce RTX 3060 Ti", "NVIDIA GeForce RTX 3060 Ti", "PCI\\VEN_10DE&DEV_2486&SUBSYS_405A1458&REV_A1\\4&31ABA5D&0&00E6", "30.0.14.9613", "OK", "Section072", 4293918720){PCI_BUS_ID = 6},
                new VideoControllerData("NVIDIA GeForce GTX 1660", "NVIDIA GeForce GTX 1660", "PCI\\VEN_10DE&DEV_2184&SUBSYS_86BD1043&REV_A1\\4&324D93F4&0&00E1", "30.0.14.9613", "OK", "Section056", 4293918720){PCI_BUS_ID = 4},
                new VideoControllerData("NVIDIA GeForce RTX 2060", "NVIDIA GeForce RTX 2060", "PCI\\VEN_10DE&DEV_1F08&SUBSYS_37D11458&REV_A1\\4&2D78AB8F&0&0008", "30.0.14.9613", "OK", "Section001", 4293918720){PCI_BUS_ID = 1},
            };
            var rawDetection = "{\"ADLErrorString\":\"ADL no AMD GPU's detected | \",\"AMDBusIDVersionPairs\":[],\"ErrorString\":\"\",\"Platforms\":[{\"Devices\":[{\"BUS_ID\":6,\"DeviceID\":0,\"_CL_DEVICE_BOARD_NAME_AMD\":\"\",\"_CL_DEVICE_GLOBAL_MEM_SIZE\":8589410304,\"_CL_DEVICE_NAME\":\"NVIDIA GeForce RTX 3060 Ti\",\"_CL_DEVICE_TYPE\":\"GPU\",\"_CL_DEVICE_VENDOR\":\"NVIDIA Corporation\",\"_CL_DEVICE_VERSION\":\"OpenCL 3.0 CUDA\",\"_CL_DRIVER_VERSION\":\"496.13\"},{\"BUS_ID\":1,\"DeviceID\":1,\"_CL_DEVICE_BOARD_NAME_AMD\":\"\",\"_CL_DEVICE_GLOBAL_MEM_SIZE\":6442123264,\"_CL_DEVICE_NAME\":\"NVIDIA GeForce RTX 2060\",\"_CL_DEVICE_TYPE\":\"GPU\",\"_CL_DEVICE_VENDOR\":\"NVIDIA Corporation\",\"_CL_DEVICE_VERSION\":\"OpenCL 3.0 CUDA\",\"_CL_DRIVER_VERSION\":\"496.13\"},{\"BUS_ID\":4,\"DeviceID\":2,\"_CL_DEVICE_BOARD_NAME_AMD\":\"\",\"_CL_DEVICE_GLOBAL_MEM_SIZE\":6442123264,\"_CL_DEVICE_NAME\":\"NVIDIA GeForce GTX 1660\",\"_CL_DEVICE_TYPE\":\"GPU\",\"_CL_DEVICE_VENDOR\":\"NVIDIA Corporation\",\"_CL_DEVICE_VERSION\":\"OpenCL 3.0 CUDA\",\"_CL_DRIVER_VERSION\":\"496.13\"}],\"PlatformName\":\"NVIDIA CUDA\",\"PlatformNum\":0,\"PlatformVendor\":\"NVIDIA Corporation\"}],\"Status\":\"OK\"}";
            var detectionObject = JsonConvert.DeserializeObject<OpenCLDeviceDetectionResult>(rawDetection);
            var result = AMDDetector.ConvertOpenCLResultToList(videoControllerDatas, (rawDetection, detectionObject));
            Assert.IsTrue(result.success, tl.label());
            Assert.IsTrue(result.list.Count == 0, tl.label());
        }
        [TestMethod]
        public void TestParseAMDDevices_MixedRig()
        {
            var tl = new TestLabel { };
            List<VideoControllerData> videoControllerDatas = new List<VideoControllerData>()
            {
                new VideoControllerData("AMD Radeon(TM) Graphics", "AMD Radeon(TM) Graphics", "PCI\\VEN_1002&DEV_1638&SUBSYS_17221043&REV_C4\\4&12C9051D&0&0041", "30.0.13002.1001", "OK", "ati2mtag_Cezanne", 536870912){PCI_BUS_ID = 4},
                new VideoControllerData("NVIDIA GeForce RTX 3060 Laptop GPU", "NVIDIA GeForce RTX 3060 Laptop GPU", "PCI\\VEN_1002&DEV_1638&SUBSYS_17221043&REV_C4\\4&12C9051D&0&0041", "30.0.15.1215", "OK", "Section147", 4293918720){PCI_BUS_ID = 1},
            };
            var rawDetection = "{\"ADLErrorString\":\"\",\"AMDBusIDVersionPairs\":[{\"ADLRetCode\":0,\"AdrenalinVersion\":\"21.30.02.01\",\"BUS_ID\":4,\"FunctionCall\":3}],\"ErrorString\":\"\",\"Platforms\":[{\"Devices\":[{\"BUS_ID\":1,\"DeviceID\":0,\"_CL_DEVICE_BOARD_NAME_AMD\":\"\",\"_CL_DEVICE_GLOBAL_MEM_SIZE\":6441926656,\"_CL_DEVICE_NAME\":\"NVIDIA GeForce RTX 3060 Laptop GPU\",\"_CL_DEVICE_TYPE\":\"GPU\",\"_CL_DEVICE_VENDOR\":\"NVIDIA Corporation\",\"_CL_DEVICE_VERSION\":\"OpenCL 3.0 CUDA\",\"_CL_DRIVER_VERSION\":\"512.15\"}],\"PlatformName\":\"NVIDIA CUDA\",\"PlatformNum\":0,\"PlatformVendor\":\"NVIDIA Corporation\"},{\"Devices\":[{\"BUS_ID\":4,\"DeviceID\":0,\"_CL_DEVICE_BOARD_NAME_AMD\":\"AMD Radeon(TM) Graphics\",\"_CL_DEVICE_GLOBAL_MEM_SIZE\":6538919936,\"_CL_DEVICE_NAME\":\"gfx90c\",\"_CL_DEVICE_TYPE\":\"GPU\",\"_CL_DEVICE_VENDOR\":\"Advanced Micro Devices, Inc.\",\"_CL_DEVICE_VERSION\":\"OpenCL 2.0 AMD-APP (3302.6)\",\"_CL_DRIVER_VERSION\":\"3302.6 (PAL,HSAIL)\"}],\"PlatformName\":\"AMD Accelerated Parallel Processing\",\"PlatformNum\":1,\"PlatformVendor\":\"Advanced Micro Devices, Inc.\"}],\"Status\":\"OK\"}";
            var detectionObject = JsonConvert.DeserializeObject<OpenCLDeviceDetectionResult>(rawDetection);
            var result = AMDDetector.ConvertOpenCLResultToList(videoControllerDatas, (rawDetection, detectionObject));
            Assert.IsTrue(result.success, tl.label());
            Assert.IsTrue(result.list.Count == 1);
        }
    }
}
