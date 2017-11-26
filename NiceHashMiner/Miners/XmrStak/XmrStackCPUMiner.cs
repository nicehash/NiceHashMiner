using Newtonsoft.Json.Linq;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners {

    public class XmrStackMainMinerConfig : XmrStakConfig
    {
        public XmrStackMainMinerConfig(string poolAddr, string wallet, int port)
            : base(poolAddr, wallet, port) {
        }

        /*
         * use_slow_memory defines our behaviour with regards to large pages. There are three possible options here:
         * always  - Don't even try to use large pages. Always use slow memory.
         * warn    - We will try to use large pages, but fall back to slow memory if that fails.
         * no_mlck - This option is only relevant on Linux, where we can use large pages without locking memory.
         *           It will never use slow memory, but it won't attempt to mlock
         * never   - If we fail to allocate large pages we will print an error and exit.
         */
        public string use_slow_memory = "warn";

        /*
         * NiceHash mode
         * nicehash_nonce - Limit the noce to 3 bytes as required by nicehash. This cuts all the safety margins, and
         *                  if a block isn't found within 30 minutes then you might run into nonce collisions. Number
         *                  of threads in this mode is hard-limited to 32.
         */
        public readonly bool nicehash_nonce = true; // 

        /*
         * Manual hardware AES override
         *
         * Some VMs don't report AES capability correctly. You can set this value to true to enforce hardware AES or 
         * to false to force disable AES or null to let the miner decide if AES is used.
         * 
         * WARNING: setting this to true on a CPU that doesn't support hardware AES will crash the miner.
         */
        public readonly bool? aes_override = null;
    }
    public class XmrStackCPUMinerConfig
    {
        public XmrStackCPUMinerConfig(int numberOfthreads)
        {
            cpu_thread_num = numberOfthreads;
        }

        public void Inti_cpu_threads_conf(bool low_power_mode, bool no_prefetch, bool affine_to_cpu, bool isHyperThreading)
        {
            cpu_threads_conf = new List<JObject>();
            if (isHyperThreading)
            {
                for (int i_cpu = 0; i_cpu < cpu_thread_num; ++i_cpu)
                {
                    cpu_threads_conf.Add(JObject.FromObject(new { low_power_mode = low_power_mode, no_prefetch = no_prefetch, affine_to_cpu = i_cpu * 2 }));
                }
            }
            else
            {
                for (int i_cpu = 0; i_cpu < cpu_thread_num; ++i_cpu)
                {
                    if (affine_to_cpu)
                    {
                        cpu_threads_conf.Add(JObject.FromObject(new { low_power_mode = low_power_mode, no_prefetch = no_prefetch, affine_to_cpu = i_cpu }));
                    }
                    else
                    {
                        cpu_threads_conf.Add(JObject.FromObject(new { low_power_mode = low_power_mode, no_prefetch = no_prefetch, affine_to_cpu = false }));
                    }
                }
            }
        }
        /* 
         * Number of threads. You can configure them below. Cryptonight uses 2MB of memory, so the optimal setting 
         * here is the size of your L3 cache divided by 2. Intel mid-to-high end desktop processors have 2MB of L3
         * cache per physical core. Low end cpus can have 1.5 or 1 MB while Xeons can have 2, 2.5 or 3MB per core.
         */
        public readonly int cpu_thread_num;

        /*
         * Thread configuration for each thread. Make sure it matches the number above.
         * low_power_mode - This mode will double the cache usage, and double the single thread performance. It will 
         *                  consume much less power (as less cores are working), but will max out at around 80-85% of 
         *                  the maximum performance.
         *
         * no_prefetch -    This mode meant for large pages only. It will generate an error if running on slow memory
         *                  Some sytems can gain up to extra 5% here, but sometimes it will have no difference or make
         *                  things slower.
         *
         * affine_to_cpu -  This can be either false (no affinity), or the CPU core number. Note that on hyperthreading 
         *                  systems it is better to assign threads to physical cores. On Windows this usually means selecting 
         *                  even or odd numbered cpu numbers. For Linux it will be usually the lower CPU numbers, so for a 4 
         *                  physical core CPU you should select cpu numbers 0-3.
         *
         */
        public List<JObject> cpu_threads_conf;
        //"cpu_threads_conf" : [ 
        //    { "low_power_mode" : false, "no_prefetch" : false, "affine_to_cpu" : 0 },
        //    { "low_power_mode" : false, "no_prefetch" : false, "affine_to_cpu" : 1 },
        //],

    }

    public class XmrDisableGPUConf
    {
        public List<JObject> gpu_threads_conf = new List<JObject>();
        public readonly int platform_index = 0;
    }

    public class XmrStackCPUMiner : XmrStak
    {
        public XmrStackCPUMiner()
            : base("XmrStackCPUMiner") {
            this.ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 3600000; // 1hour
        }

        protected override void prepareConfigFile(string pool, string wallet) {
            if (this.MiningSetup.MiningPairs.Count > 0) {
                try {
                    bool IsHyperThreadingEnabled = this.MiningSetup.MiningPairs[0].CurrentExtraLaunchParameters.Contains("enable_ht=true");
                    int numTr = ExtraLaunchParametersParser.GetThreadsNumber(this.MiningSetup.MiningPairs[0]);
                    if (IsHyperThreadingEnabled) {
                        numTr /= 2;
                    }
                    var cpuconf = new XmrStackCPUMinerConfig(numTr);
                    var no_prefetch = ExtraLaunchParametersParser.GetNoPrefetch(MiningSetup.MiningPairs[0]);
                    //config.Inti_cpu_threads_conf(false, false, true, ComputeDeviceManager.Avaliable.IsHyperThreadingEnabled);
                    cpuconf.Inti_cpu_threads_conf(false, no_prefetch, false, IsHyperThreadingEnabled);
                    var cpuConfJson = JObject.FromObject(cpuconf);
                    string writeStrCPU = cpuConfJson.ToString();
                    int start = writeStrCPU.IndexOf("{");
                    int end = writeStrCPU.LastIndexOf("}");
                    writeStrCPU = writeStrCPU.Substring(start + 1, end - 1);
                    System.IO.File.WriteAllText(WorkingDirectory + "cpu.txt", writeStrCPU);
                    var config = new XmrStackMainMinerConfig(pool, wallet, this.APIPort);
                    var confJson = JObject.FromObject(config);
                    string writeStr = confJson.ToString();
                    start = writeStr.IndexOf("{");
                    end = writeStr.LastIndexOf("}");
                    writeStr = writeStr.Substring(start + 1, end - 1);
                    System.IO.File.WriteAllText(WorkingDirectory + GetConfigFileName(), writeStr);
                    var disableGPU = new XmrDisableGPUConf();
                    var disableGPUconf = JObject.FromObject(disableGPU);
                    writeStr = disableGPUconf.ToString();
                    start = writeStr.IndexOf("{");
                    end = writeStr.LastIndexOf("}");
                    writeStr = writeStr.Substring(start + 1, end - 1);
                    System.IO.File.WriteAllText(WorkingDirectory + "nvidia.txt", writeStr);
                    System.IO.File.WriteAllText(WorkingDirectory + "amd.txt", writeStr);


                }
                catch (Exception e) {
                Debug.WriteLine(e.Message);
                }
           }
        }

        protected override NiceHashProcess _Start() {
            NiceHashProcess P = base._Start();

            var AffinityMask = MiningSetup.MiningPairs[0].Device.AffinityMask;
            if (AffinityMask != 0 && P != null)
                CPUID.AdjustAffinity(P.Id, AffinityMask);

            return P;
        }

        protected override Process BenchmarkStartProcess(string CommandLine) {
            Process BenchmarkHandle = base.BenchmarkStartProcess(CommandLine);

            var AffinityMask = MiningSetup.MiningPairs[0].Device.AffinityMask;
            if (AffinityMask != 0 && BenchmarkHandle != null)
                CPUID.AdjustAffinity(BenchmarkHandle.Id, AffinityMask);

            return BenchmarkHandle;
        }
    }
}
