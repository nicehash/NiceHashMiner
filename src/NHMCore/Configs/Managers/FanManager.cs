using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs.Managers
{
    public class FanManager
    {
        private FanManager() { }
        public static FanManager Instance { get; } = new FanManager();
        private readonly string _TAG = "FanManager";
    }
}
