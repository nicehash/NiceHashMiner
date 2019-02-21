using System;
using System.Collections.Generic;
using System.Threading;
using MinerPlugin.Toolkit;
using MinerPluginLoader;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;


// alias
using TimersTimer = System.Timers.Timer;

namespace TempMinerRunner
{
    class Program
    {
        static void execBenchmark(string[] args)
        {
            var binPath = @"D:\Programming\NiceHashMinerLegacy\Release\bin\ccminer_tpruvot\ccminer.exe";
            var binCwd = @"D:\Programming\NiceHashMinerLegacy\Release\bin\ccminer_tpruvot\";
            var commandLine = "--algo=keccak --benchmark --time-limit 60 --devices 0";

            if (args.Length >= 3)
            {
                binPath = args[0];
                binCwd = args[1];
                commandLine = args[2];
            }

            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);
            bp.CheckData = (string data) => {
                Console.Write($"DATA_START________: {data}");
                Console.WriteLine(" ________DATA_END");
                return MinerToolkit.TryGetHashrateAfter(data, "Benchmark:");
                //return (0, false);
            };
            var stopBenchmark = new CancellationTokenSource();

            //var timeoutTimer = new TimersTimer(250);
            //timeoutTimer.Elapsed += (s, e) => stopBenchmark.Cancel(); // we can only cancel here so we are fine
            //timeoutTimer.AutoReset = false; // single shot
            //timeoutTimer.Start();

            var t = MinerToolkit.WaitBenchmarkResult(bp, TimeSpan.FromSeconds(150), TimeSpan.FromMilliseconds(150), stopBenchmark.Token);
            t.Wait();
            Console.WriteLine("EXITING APP");
        }

        static void execMining(string[] args)
        {
            var binPath = @"D:\Programming\NiceHashMinerLegacy\Release\bin\ccminer_tpruvot\ccminer.exe";
            var binCwd = @"D:\Programming\NiceHashMinerLegacy\Release\bin\ccminer_tpruvot\";
            //var commandLine = "--algo=keccak --benchmark --time-limit 60 --devices 0";
            var commandLine = "--algo=lyra2v3 --url=stratum+tcp://lyra2rev3.eu.nicehash.com:3373 --userpass=37oPP9rByzBez2rkBxcL9FVGHWxW31JCyx.nhmlworker:x  --api-bind=4000 --devices  1";

            if (args.Length >= 3)
            {
                binPath = args[0];
                binCwd = args[1];
                commandLine = args[2];
            }

            var mp = MinerToolkit.CreateMiningProcess(binPath, binCwd, commandLine);
            mp.Exited += (s, e) => Console.WriteLine("MINER IS DYING");
            mp.Start();
            try
            {
                //mp.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"PROCESS threw: {e.Message}");
            }

            var stopBenchmark = new CancellationTokenSource();

            //var timeoutTimer = new TimersTimer(250);
            //timeoutTimer.Elapsed += (s, e) => stopBenchmark.Cancel(); // we can only cancel here so we are fine
            //timeoutTimer.AutoReset = false; // single shot
            //timeoutTimer.Start();

            Thread.Sleep(5000);
            mp.Kill();
            Thread.Sleep(5000);

            Console.WriteLine("EXITING APP");
        }

        static void run_ccminer()
        {
            var username = "37oPP9rByzBez2rkBxcL9FVGHWxW31JCyx.testing";
            var ccminer = new CCMinerBase.CCMinerBase();
            ccminer.InitMiningLocationAndUsername("eu", username);
            var miningPairs = new List<(BaseDevice, Algorithm)>();


            var algo = new Algorithm("ccminerID", AlgorithmType.NeoScrypt);
            var cudaDevice01 = new BaseDevice(DeviceType.NVIDIA, "UUID01", "FIRST GPU", 0);
            var cudaDevice02 = new BaseDevice(DeviceType.NVIDIA, "UUID02", "SECOND GPU", 1);
            miningPairs.Add((cudaDevice01, algo));
            miningPairs.Add((cudaDevice02, algo));
            ccminer.InitMiningPairs(miningPairs);


            ccminer.StartMining();
            try
            {
                //mp.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine($"PROCESS threw: {e.Message}");
            }

            Console.WriteLine("Waiting before we stop the miner");
            Thread.Sleep(35 * 1000);
            try
            {
                var ret = ccminer.GetMinerStatsDataAsync();
                ret.Wait();
                Console.WriteLine("API GOT");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Thread.Sleep(60 * 60 * 1000);
            ccminer.StopMining();
            Console.WriteLine("Miner STOP called");
            Thread.Sleep(5000);

            Console.WriteLine("EXITING APP");
        }



        //static void Main(string[] args)
        //{
        //    var username = "37oPP9rByzBez2rkBxcL9FVGHWxW31JCyx.testing";
        //    var cpuminer = new GMinerPlugin.GMiner();
        //    cpuminer.InitMiningLocationAndUsername("eu", username);
        //    var miningPairs = new List<(BaseDevice, Algorithm)>();


        //    var algo = new Algorithm("minerID", AlgorithmType.Beam);
        //    var cpuDevice01 = new BaseDevice(DeviceType.NVIDIA, "GPU01", "FIRST GPU", 0);
        //    miningPairs.Add((cpuDevice01, algo));
        //    cpuminer.InitMiningPairs(miningPairs);

        //    //var benchTask = cpuminer.StartBenchmark(CancellationToken.None);
        //    //Console.WriteLine($"BENCHMARKING");
        //    //benchTask.Wait();
        //    //var (benchedSpeed, isBenchmarkSuccess) = benchTask.Result;
        //    //Console.WriteLine($"BENCHMARK RESULT {benchedSpeed} {isBenchmarkSuccess}");

        //    cpuminer.StartMining();
        //    try
        //    {
        //        //mp.WaitForExit();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"PROCESS threw: {e.Message}");
        //    }

        //    Console.WriteLine("Waiting before we stop the miner");
        //    Thread.Sleep(35 * 1000);
        //    try {
        //        var ret = cpuminer.GetSummaryAsync();
        //        ret.Wait();
        //        Console.WriteLine("API GOT");
        //    } catch(Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }

        //    Thread.Sleep(60 * 60 * 1000);
        //    cpuminer.StopMining();
        //    Console.WriteLine("Miner STOP called");
        //    Thread.Sleep(5000);

        //    Console.WriteLine("EXITING APP");
        //}

        static void Main(string[] args)
        {
            var supportedDevices = new List<BaseDevice> {
                new CPUDevice(new BaseDevice(DeviceType.CPU, "uuid01", "name01", 0), 12, true, 0),
                new CUDADevice(new BaseDevice(DeviceType.NVIDIA, "cudauuid01", "cudaname01", 0), 1, 50, 5,0)
            };
            CUDADevice.INSTALLED_NVIDIA_DRIVERS = "SOME SET VERSION";
            MinerPluginHost.LoadPlugins(@"D:\work\temp\dlls");
            foreach (var (uuid, plugin) in MinerPluginHost.MinerPlugin)
            {
                var supported = plugin.GetSupportedAlgorithms(supportedDevices);
                if (supported.Count == 0)
                {
                    Console.WriteLine($"{uuid} No Support");
                }
                else
                {
                    Console.WriteLine($"{uuid} Has Support");
                }
            }
            
        }
    }
}
