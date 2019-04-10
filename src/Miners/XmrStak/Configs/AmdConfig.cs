using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmrStak.Configs
{
#pragma warning disable IDE1006 // Naming Styles

    [Serializable]
    public class AmdThreadsConfItem
    {
        public int index = 0;
        public int intensity = 0;
        public int worksize = 0;
        public bool affine_to_cpu = false;
        public int strided_index = 0;
        public int mem_chunk = 0;
        public int unroll = 0;
        public bool comp_mode = false;
        public int interleave = 0;
    }

    [Serializable]
    public class AmdConfig
    {
        public List<AmdThreadsConfItem> gpu_threads_conf;
        public int auto_tune = 0;
        public int platform_index;
    }
#pragma warning restore IDE1006 // Naming Styles

}
