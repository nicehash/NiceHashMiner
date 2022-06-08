using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHM.DeviceDetection.OpenCL.Models;
using NHM.DeviceDetection.AMD;
using NHM.DeviceDetection.WMI;
using System.Linq;
using NHM.Common.Enums;
using Newtonsoft.Json.Linq;

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
        
        internal class DetectionTestData
        {
            public OpenCLDeviceDetectionResult OpenCL { get; set; }
            public List<VideoControllerData> WMI_VideoController { get; set; }
        }

        [TestMethod]
        public void TestParseAMDDevicesToList_DuplicateDevice()
        {
            var tl = new TestLabel { };

            var detectionStr = Paths.LoadTextFile(@"AMDDetection\data\detection001.json");
            var detectionData = JsonConvert.DeserializeObject<DetectionTestData>(detectionStr);

            var rawDetectionText = JObject.Parse(detectionStr)["OpenCL"].ToString();
            var videoControllerDatas = detectionData.WMI_VideoController;
            var detectionObject = detectionData.OpenCL;

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
            var detectionStr = Paths.LoadTextFile(@"AMDDetection\data\detection002.json");
            var detectionData = JsonConvert.DeserializeObject<DetectionTestData>(detectionStr);

            var rawDetectionText = JObject.Parse(detectionStr)["OpenCL"].ToString();
            var videoControllerDatas = detectionData.WMI_VideoController;
            var detectionObject = detectionData.OpenCL;

            var result = AMDDetector.ConvertOpenCLResultToList(videoControllerDatas, (rawDetectionText, detectionObject));
            Assert.IsTrue(result.success, tl.label());
            Assert.IsTrue(result.list.Count == 0, tl.label());
        }
        [TestMethod]
        public void TestParseAMDDevices_MixedRig()
        {
            var tl = new TestLabel { };
            var detectionStr = Paths.LoadTextFile(@"AMDDetection\data\detection003.json");
            var detectionData = JsonConvert.DeserializeObject<DetectionTestData>(detectionStr);

            var rawDetectionText = JObject.Parse(detectionStr)["OpenCL"].ToString();
            var videoControllerDatas = detectionData.WMI_VideoController;
            var detectionObject = detectionData.OpenCL;

            var result = AMDDetector.ConvertOpenCLResultToList(videoControllerDatas, (rawDetectionText, detectionObject));
            Assert.IsTrue(result.success, tl.label());
            Assert.IsTrue(result.list.Count == 1, tl.label());
            Assert.AreEqual(result.list.First().DeviceType, DeviceType.AMD, tl.label());
        }
    }
}
