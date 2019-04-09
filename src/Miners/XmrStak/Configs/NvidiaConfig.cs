using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmrStak.Configs
{
#pragma warning disable IDE1006 // Naming Styles

    [Serializable]
    public class NvidiaThreadsConfItem
    {
        public int index = 0;
        public int threads = 0;
        public int blocks = 0;
        public int bfactor = 0;
        public int bsleep = 0;
        public bool affine_to_cpu = false;
        public int sync_mode = 0;
        public int mem_mode = 0;
    }

    [Serializable]
    public class NvidiaConfig
    {
        public List<NvidiaThreadsConfItem> gpu_threads_conf;
    }
#pragma warning restore IDE1006 // Naming Styles

}
