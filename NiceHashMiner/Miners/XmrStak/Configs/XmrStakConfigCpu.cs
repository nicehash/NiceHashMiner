using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NiceHashMiner.Miners.XmrStak.Configs
{
    public class XmrStakConfigCpu
    {
        private readonly int cpu_thread_num;

        public XmrStakConfigCpu(int numberOfthreads)
        {
            cpu_thread_num = numberOfthreads;
        }

        public void Inti_cpu_threads_conf(bool low_power_mode, bool no_prefetch, bool affine_to_cpu,
            bool isHyperThreading)
        {
            cpu_threads_conf = new List<JObject>();
            if (isHyperThreading)
            {
                for (int i_cpu = 0; i_cpu < cpu_thread_num; ++i_cpu)
                {
                    cpu_threads_conf.Add(JObject.FromObject(new
                    {
                        low_power_mode = low_power_mode,
                        no_prefetch = no_prefetch,
                        affine_to_cpu = i_cpu * 2
                    }));
                }
            }
            else
            {
                for (int i_cpu = 0; i_cpu < cpu_thread_num; ++i_cpu)
                {
                    if (affine_to_cpu)
                    {
                        cpu_threads_conf.Add(JObject.FromObject(new
                        {
                            low_power_mode = low_power_mode,
                            no_prefetch = no_prefetch,
                            affine_to_cpu = i_cpu
                        }));
                    }
                    else
                    {
                        cpu_threads_conf.Add(JObject.FromObject(new
                        {
                            low_power_mode = low_power_mode,
                            no_prefetch = no_prefetch,
                            affine_to_cpu = false
                        }));
                    }
                }
            }
        }

        /*
         * Thread configuration for each thread. Make sure it matches the number above.
         * low_power_mode - This mode will double the cache usage, and double the single thread performance. It will 
         *                  consume much less power (as less cores are working), but will max out at around 80-85% of 
         *                  the maximum performance.
         *
         * no_prefetch -    Some sytems can gain up to extra 5% here, but sometimes it will have no difference or make
         *                  things slower.
         *
         * affine_to_cpu -  This can be either false (no affinity), or the CPU core number. Note that on hyperthreading 
         *                  systems it is better to assign threads to physical cores. On Windows this usually means selecting 
         *                  even or odd numbered cpu numbers. For Linux it will be usually the lower CPU numbers, so for a 4 
         *                  physical core CPU you should select cpu numbers 0-3.
         *
         * On the first run the miner will look at your system and suggest a basic configuration that will work,
         * you can try to tweak it from there to get the best performance.
         * 
         * A filled out configuration should look like this:
         * "cpu_threads_conf" :
         * [ 
         *      { "low_power_mode" : false, "no_prefetch" : true, "affine_to_cpu" : 0 },
         *      { "low_power_mode" : false, "no_prefetch" : true, "affine_to_cpu" : 1 },
         * ],
         */

        public List<JObject> cpu_threads_conf = new List<JObject>();
    }
}
