using Microsoft.VisualStudio.TestTools.UnitTesting;
using NanoMiner;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miners
{
    [TestClass]
    public class NanoMinerTest
    {
        
        [TestMethod]
        public void ParseJsonData()
        {
        var result= "{\n\"Algorithms\":[\n{\n\"CryptoNightR\":{\n\"CurrentPool\":\"cryptonightr.eu.nicehash.com:3375\",\n\"GPU0\":{\n\"Accepted\":0,\n\"Denied\":0,\n\"Hashrate\":\"6.302745e+02\"\n},\"GPU1\":{\n\"Accepted\":0,\n\"Denied\":0,\n\"Hashrate\":\"2.302745e+02\"\n},\n\"ReconnectionCount\":0,\n\"Total\":{\n\"Accepted\":0,\n\"Denied\":0,\n\"Hashrate\":\"8.605490e+02\"\n}\n}\n}\n],\n\"Devices\":[\n{\n\"GPU0\":{\n\"Name\":\"GeForceGTX1070Ti\",\n\"Platform\":\"CUDA\",\n\"Pci\":\"01:00.0\",\n\"Temperature\":60,\n\"Power\":83.7\n},\"GPU1\":{\n\"Name\":\"GeForceGTX10606GB\",\n\"Platform\":\"CUDA\",\n\"Pci\":\"02:00.0\",\n\"Temperature\":60,\n\"Power\":83.7\n}\n}\n],\n\"WorkTime\":19\n}";
            var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);
            var gpus = summary.Devices.ToString();
            var algo = summary.Algorithms.ToString();

            gpus = gpus.Remove(0, 1).Remove(gpus.Length - 1, 1);
            var listOfGpuNames = new List<string>();

            var nek = "[\r\n  {\r\n    \"GPU0\": {\r\n      \"Name\": \"GeForceGTX1070Ti\",\r\n      \"Platform\": \"CUDA\",\r\n      \"Pci\": \"01:00.0\",\r\n      \"Temperature\": 60,\r\n      \"Power\": 83.7\r\n    },\r\n    \"GPU1\": {\r\n      \"Name\": \"GeForceGTX10606GB\",\r\n      \"Platform\": \"CUDA\",\r\n      \"Pci\": \"02:00.0\",\r\n      \"Temperature\": 60,\r\n      \"Power\": 83.7\r\n    }\r\n  }\r\n]";
            //foreach(var gpu in gpus)
            //{
            //    var name = gpu.Value.Name;
            //    listOfGpuNames.Add(name);
            //    var platform = gpu.Value.Platform;
            //    var busId = gpu.Value.Pci;
            //}
        }

    }
}
