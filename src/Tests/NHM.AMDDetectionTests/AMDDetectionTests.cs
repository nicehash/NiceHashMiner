using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using static NHM.DeviceDetection.DeviceDetection;
using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace NHM.AMDDetectionTests
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
        public async Task TestParseAMDDevicesToList()
        {
            var tl = new TestLabel { };
            var privateObjDevDetection = new PrivateType(typeof(DeviceDetection.DeviceDetection));
            await (Task)privateObjDevDetection.InvokeStatic("DetectWMIVideoControllers");// await??????
            var vidControllers = privateObjDevDetection.GetStaticFieldOrProperty("DetectionResult");

            
            //var result = AMDDetector.ConvertOpenCLResultToList();

        }
    }
}
