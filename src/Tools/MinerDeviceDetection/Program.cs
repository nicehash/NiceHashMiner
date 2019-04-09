using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerDeviceDetection
{
    class Program
    {
        static Dictionary<string, string> DirSearch(string dir)
        {
            var miners = new Dictionary<string, string>();
            try
            {
                foreach (string miner in Directory.GetFiles(dir, "*.exe", SearchOption.AllDirectories))
                {
                    var folderName = Path.GetFileName(Path.GetDirectoryName(miner));
                    if (folderName != "bin" && folderName != "bin_3rdparty" && folderName != "ethlargement")
                    {
                        if (!miners.ContainsKey(folderName))
                        miners.Add(folderName, miner);
                    }
                }
            }
            catch (Exception e)
            {
            }
            return miners;
        }

        static string MinerOutput(string path, string arguments)
        {
            string output = "";
            try
            {
                Process getDevices = new Process
                {StartInfo ={
                                FileName = path,
                                Arguments = arguments,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                }};
                getDevices.Start();
                output = getDevices.StandardOutput.ReadToEnd();
                getDevices.WaitForExit();
            }
            catch (Exception e)
            {
            }
            return output;
        }

        static string MinerErrorOutput(string path, string arguments)
        {
            string output = "";
            try
            {
                Process getDevices = new Process
                {
                    StartInfo ={
                                FileName = path,
                                Arguments = arguments,
                                UseShellExecute = false,
                                RedirectStandardError = true
                }
                };
                getDevices.Start();
                output = getDevices.StandardError.ReadToEnd();
                getDevices.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
            return output;
        }

        static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            List<string> binPaths = new List<string>() { "bin", "bin_3rdparty" };
            var binData = new Dictionary<string, string>();
            foreach (var path in binPaths)
            {
                var tmp = DirSearch(Path.Combine(root, path));
                foreach (var minerData in tmp)
                {
                    binData.Add(minerData.Key, minerData.Value);
                }
            }
            Dictionary<string, string> minerDeviceDetection = new Dictionary<string, string>();
            Dictionary<string, string> goodMiners = new Dictionary<string, string>() { {"avemore"/*sgminer*/, "--ndevs" }, {"sgminer-gm", "--ndevs" },{"sgminer-5-6-0-general", "--ndevs" },{ "gminer", "--list_devices" }, {"nbminer", "--device-info" }, {"phoenix", "-list" }, { "teamredminer", "--list_devices"}, { "ttminer", "-list"} };
            if (File.Exists("outTest.txt"))
            {
                File.Delete("outTest.txt");
            }
            if (File.Exists("mappedGpus.txt"))
            {
                File.Delete("mappedGpus.txt");
            }
            using (StreamWriter mappedMinersFile = new StreamWriter("mappedGpus.txt", true))
            {

                foreach (var miner in goodMiners)
                {
                    foreach (var bin in binData)
                    {
                        if (bin.Key == miner.Key)
                        {
                            var output = MinerOutput(bin.Value, miner.Value);
                            List<BaseDevice> tmpDeleteDevices = new List<BaseDevice>();
                            var dev = new CUDADevice(new BaseDevice(DeviceType.NVIDIA, "uuid1", "GeForce GTX 1060 6GB", 1), 2, 200, 6, 1);
                            tmpDeleteDevices.Add(dev);
                            dev = new CUDADevice(new BaseDevice(DeviceType.NVIDIA, "uuid0", "GeForce GTX 1070 Ti", 0), 1, 200, 6, 1);
                            tmpDeleteDevices.Add(dev);
                            var mappedDevices = new Dictionary<string, int>();

                            switch (bin.Key)
                            {
                                case "gminer":
                                    mappedMinersFile.WriteLine("GMiner");
                                    mappedDevices = OutputParsers.ParseGMinerOutput(output, tmpDeleteDevices);
                                    foreach (var device in tmpDeleteDevices)
                                    {
                                        foreach (var tmpDev in mappedDevices)
                                        {
                                            if (device.UUID == tmpDev.Key)
                                            {
                                                mappedMinersFile.WriteLine($"{tmpDev.Value}     {device.Name}");
                                            }
                                        }
                                    }
                                    break;
                                case "nbminer":
                                    mappedMinersFile.WriteLine("NBMiner");
                                    var errOutput = MinerErrorOutput(bin.Value, miner.Value);
                                    mappedDevices = OutputParsers.ParseNBMinerOutput(errOutput, tmpDeleteDevices);
                                    foreach (var device in tmpDeleteDevices)
                                    {
                                        foreach (var tmpDev in mappedDevices)
                                        {
                                            if (device.UUID == tmpDev.Key)
                                            {
                                                mappedMinersFile.WriteLine($"{tmpDev.Value}     {device.Name}");
                                            }
                                        }
                                    }
                                    break;
                                case "phoenix":
                                    mappedMinersFile.WriteLine("Phoenix");
                                    mappedDevices = OutputParsers.ParsePhoenixOutput(output, tmpDeleteDevices);
                                    foreach (var device in tmpDeleteDevices)
                                    {
                                        foreach (var tmpDev in mappedDevices)
                                        {
                                            if (device.UUID == tmpDev.Key)
                                            {
                                                mappedMinersFile.WriteLine($"{tmpDev.Value}     {device.Name}");
                                            }
                                        }
                                    }
                                    break;
                                case "ttminer":
                                    mappedMinersFile.WriteLine("TTMiner");
                                    mappedDevices = OutputParsers.ParseTTMinerOutput(output, tmpDeleteDevices);
                                    foreach (var device in tmpDeleteDevices)
                                    {
                                        foreach (var tmpDev in mappedDevices)
                                        {
                                            if (device.UUID == tmpDev.Key)
                                            {
                                                mappedMinersFile.WriteLine($"{tmpDev.Value}     {device.Name}");
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            minerDeviceDetection.Add(miner.Key, output);
                            using (StreamWriter file = new StreamWriter("outTest.txt", true)) { file.WriteLine(miner.Key); file.WriteLine(output); }
                        }
                    }
                }
            }
        }
    }
}
