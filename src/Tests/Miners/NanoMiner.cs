using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NanoMiner;
using Newtonsoft.Json;

namespace Miners
{
    [TestClass]
    public class NanoMinerTest
    {
        [TestMethod]
        public void ParseJsonData()
        {
            var result = "{\n\"Algorithms\":[\n{\n\"CryptoNightR\":{\n\"CurrentPool\":\"cryptonightr.eu.nicehash.com:3375\",\n\"GPU0\":{\n\"Accepted\":0,\n\"Denied\":0,\n\"Hashrate\":\"6.302745e+02\"\n},\"GPU1\":{\n\"Accepted\":0,\n\"Denied\":0,\n\"Hashrate\":\"2.302745e+02\"\n},\n\"ReconnectionCount\":0,\n\"Total\":{\n\"Accepted\":0,\n\"Denied\":0,\n\"Hashrate\":\"8.605490e+02\"\n}\n}\n}\n],\n\"Devices\":[\n{\n\"GPU0\":{\n\"Name\":\"GeForceGTX1070Ti\",\n\"Platform\":\"CUDA\",\n\"Pci\":\"01:00.0\",\n\"Temperature\":60,\n\"Power\":83.7\n},\"GPU1\":{\n\"Name\":\"GeForceGTX10606GB\",\n\"Platform\":\"CUDA\",\n\"Pci\":\"02:00.0\",\n\"Temperature\":60,\n\"Power\":83.7\n}\n}\n],\n\"WorkTime\":19\n}";
            var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

            var dev = summary.Devices;
            var listOfGpuNames = new Dictionary<string, string>();

            foreach (var apiDeviceData in dev)
            {
                foreach (var kvp in apiDeviceData)
                {
                    var devID = kvp.Key;
                    var devData = kvp.Value;
                    listOfGpuNames.Add(devID, devData.Pci);
                }
            }

            var alg = summary.Algorithms;
            foreach (var apiAlgoData in alg)
            {
                foreach (var kvp in apiAlgoData)
                {
                    var algo = kvp.Key;
                    var algoData = kvp.Value;
                    foreach (var data in algoData)
                    {
                        if (data.Key == "Total")
                        {
                            var speed = data.Value.ToString();
                            var num = JsonApiHelpers.HashrateFromApiData(speed, "TestGroup");
                        }
                    }
                }
            }
        }
        [TestMethod]
        public void ParseOutputData()
        {
            var output = new List<string>() { "GPU 0  PCI 01:00.0  8192 MB GeForce GTX 1070 Ti" , "GPU 1  PCI 0a:00.0  6144 MB GeForce GTX 1060 6GB" };
            if (output.Count != 0)
            {
                foreach (var line in output)
                {
                    if (!line.Contains("GPU")) continue;
                    var tmpArray = line.Split(new[] { "PCI" }, StringSplitOptions.RemoveEmptyEntries);
                    var fullPciId = tmpArray[1].Split(':');
                    var pciId = fullPciId[0];
                    var id = tmpArray[0].Remove(0, 3);
                    var indexID = Convert.ToInt32(id);
                    int comparePCIeBusID = int.Parse(pciId, System.Globalization.NumberStyles.HexNumber);
                    var i = 0;
                }
            }
        }
    }
}
