using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHM.DeviceDetection.OpenCL.Models;
using NHM.DeviceDetection.AMD;
using NHM.DeviceDetection.WMI;
using System.Linq;
using NHM.Common.Device;
using NHM.Common.Enums;

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
        private string ReadTestFile(string path)
        {
            try
            {
                return Paths.LoadTextFile(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return string.Empty;
        }
        [TestMethod]
        public void TestParseAMDDevicesToList_DuplicateDevice()
        {
            var tl = new TestLabel { };
            var videoControllerText = ReadTestFile(@"AMDDetection\data\videoControllers\controllers001.json");
            var rawDetectionText = ReadTestFile(@"AMDDetection\data\rawDetection\detection001.json");
            if (videoControllerText == string.Empty || rawDetectionText == string.Empty) return;
            var videoControllerDatas = JsonConvert.DeserializeObject<List<VideoControllerData>>(videoControllerText);
            var detectionObject = JsonConvert.DeserializeObject<OpenCLDeviceDetectionResult>(rawDetectionText);
            var result = AMDDetector.ConvertOpenCLResultToList(videoControllerDatas, (rawDetectionText, detectionObject));
            Assert.IsFalse(result.success, tl.label());
            var result2 = AMDDetector.ConvertOpenCLResultToListFallback(videoControllerDatas, (rawDetectionText, detectionObject), (rawDetectionText, detectionObject));
            Assert.AreEqual(2, result2.Count, tl.label());
            Assert.IsTrue(result2.Select(dev => dev.PCIeBusID).Distinct().Count() == 2, tl.label());
        }
        [TestMethod]
        public void TestParseAMDDevicesToList_NoAMD()
        {
            var tl = new TestLabel { };
            var videoControllerText = ReadTestFile(@"AMDDetection\data\videoControllers\controllers002.json");
            var rawDetectionText = ReadTestFile(@"AMDDetection\data\rawDetection\detection002.json");
            if (videoControllerText == string.Empty || rawDetectionText == string.Empty) return;
            var videoControllerDatas = JsonConvert.DeserializeObject<List<VideoControllerData>>(videoControllerText);
            var detectionObject = JsonConvert.DeserializeObject<OpenCLDeviceDetectionResult>(rawDetectionText);
            var result = AMDDetector.ConvertOpenCLResultToList(videoControllerDatas, (rawDetectionText, detectionObject));
            Assert.IsTrue(result.success, tl.label());
            Assert.IsTrue(result.list.Count == 0, tl.label());
        }
        [TestMethod]
        public void TestParseAMDDevices_MixedRig()
        {
            var tl = new TestLabel { };
            var videoControllerText = ReadTestFile(@"AMDDetection\data\videoControllers\controllers003.json");
            var rawDetectionText = ReadTestFile(@"AMDDetection\data\rawDetection\detection003.json");
            if (videoControllerText == string.Empty || rawDetectionText == string.Empty) return;
            var videoControllerDatas = JsonConvert.DeserializeObject<List<VideoControllerData>>(videoControllerText);
            var detectionObject = JsonConvert.DeserializeObject<OpenCLDeviceDetectionResult>(rawDetectionText);
            var result = AMDDetector.ConvertOpenCLResultToList(videoControllerDatas, (rawDetectionText, detectionObject));
            Assert.IsTrue(result.success, tl.label());
            Assert.IsTrue(result.list.Count == 1, tl.label());
            Assert.AreEqual(result.list.First().DeviceType, DeviceType.AMD, tl.label());
        }
    }
}
