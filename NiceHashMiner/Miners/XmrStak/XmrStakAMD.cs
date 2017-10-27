using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Parsing;
using System.Text.RegularExpressions;

namespace NiceHashMiner.Miners
{
    public class XmrStakGPUSettings
    {
        public int index = 0;
        public int intensity = 1000;
        public int worksize = 8;
        public bool affine_to_cpu = false;

        public XmrStakGPUSettings(int index, int intensity, int worksize = 8, bool affine_to_cpu = false) {
            this.index = index;
            this.intensity = intensity;
            this.worksize = worksize;
            this.affine_to_cpu = affine_to_cpu;
        }
    }
    public class XmrStakAMDConfig : XmrStakConfig
    {
        public XmrStakAMDConfig(string poolAddr, string wallet, int port)
            : base(poolAddr, wallet, port) {
        }

        public void Initialize_gpu_threads_conf(List<XmrStakGPUSettings> gpuSettings) {
            gpu_threads_conf = new List<JObject>();
            foreach (var settings in gpuSettings) {
                gpu_threads_conf.Add(JObject.FromObject(settings));
            }
            gpu_thread_num = gpuSettings.Count;
        }
        /* 
         * Number of GPUs that you have in your system. Each GPU will get its own CPU thread.
         */
        public int gpu_thread_num = 6;

        /*
         * GPU configuration. You should play around with intensity and worksize as the fastest settings will vary.
         *      index    - GPU index number usually starts from 0
         *  intensity    - Number of parallel GPU threads (nothing to do with CPU threads)
         *   worksize    - Number of local GPU threads (nothing to do with CPU threads)
         * affine_to_cpu - This will affine the thread to a CPU. This can make a GPU miner play along nicer with a CPU miner.
         */
        public List<JObject> gpu_threads_conf;
         /*
        "gpu_threads_conf" : [
        { "index" : 0, "intensity" : 1000, "worksize" : 8, "affine_to_cpu" : false },
        { "index" : 1, "intensity" : 1000, "worksize" : 8, "affine_to_cpu" : false },
        { "index" : 2, "intensity" : 1000, "worksize" : 8, "affine_to_cpu" : false },
        { "index" : 3, "intensity" : 1000, "worksize" : 8, "affine_to_cpu" : false },
        { "index" : 4, "intensity" : 1000, "worksize" : 8, "affine_to_cpu" : false },
        { "index" : 5, "intensity" : 1000, "worksize" : 8, "affine_to_cpu" : false },
        ],*/

        /*
         * Platform index. This will be 0 unless you have different OpenCL platform - eg. AMD and Intel.
         */
        public int platform_index = 0;
    }
    class XmrStakAMD : XmrStak
    {
        public XmrStakAMD()
            : base("XmrStakAMD") {
            ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 5 * 60 * 1000;  // 5 minutes
        }
        
        protected override void prepareConfigFile(string pool, string wallet) {
            try {
                var config = new XmrStakAMDConfig(pool, wallet, APIPort);
                var gpuConfigs = new List<XmrStakGPUSettings>();
                foreach (var pair in MiningSetup.MiningPairs) {
                    var intensities = ExtraLaunchParametersParser.GetIntensityStak(pair);
                    if (intensities.Count <= 0) intensities.Add(1000);
                    gpuConfigs.AddRange(intensities.Select(intensity =>
                        new XmrStakGPUSettings(pair.Device.ID, intensity)));
                }
                config.Initialize_gpu_threads_conf(gpuConfigs);
                var serializer = new JsonSerializer();
                serializer.TypeNameHandling = TypeNameHandling.All;
                var confJson = JObject.FromObject(config);
                var writeStr = confJson.ToString();
                var start = writeStr.IndexOf("{");
                int end = writeStr.LastIndexOf("}");
                writeStr = writeStr.Substring(start + 1, end - 1);
                System.IO.File.WriteAllText(WorkingDirectory + GetConfigFileName(), writeStr);
            } catch { }
        }

        public override async Task<APIData> GetSummaryAsync() {
            string resp;
            APIData ad = new APIData(MiningSetup.CurrentAlgorithmType);

            string DataToSend = GetHttpRequestNHMAgentStrin("h");

            resp = await GetAPIDataAsync(APIPort, DataToSend, false, true);
            if (resp == null) {
                Helpers.ConsolePrint(MinerTAG(), ProcessTag() + " summary is null");
                _currentMinerReadStatus = MinerAPIReadStatus.NONE;
                return null;
            }
            const string Totals = "Totals:";
            const string Highest = "Highest:";
            int start_i = resp.IndexOf(Totals);
            int end_i = resp.IndexOf(Highest);
            if (start_i > -1 && end_i > -1) {
                string sub_resp = resp.Substring(start_i, end_i - start_i);
                sub_resp = sub_resp.Replace(Totals, "");
                sub_resp = sub_resp.Replace(Highest, "");
                sub_resp = Regex.Replace(sub_resp, "<.*?>", string.Empty);  // Remove HTML tags
                var strings = sub_resp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in strings) {
                    if (double.TryParse(s, out var speed)) {
                        _currentMinerReadStatus = MinerAPIReadStatus.GOT_READ;
                        ad.Speed = speed;
                        break;
                    }
                }
            }
            // check if speed zero
            if (ad.Speed == 0) _currentMinerReadStatus = MinerAPIReadStatus.READ_SPEED_ZERO;

            return ad;
        }
    }
}
