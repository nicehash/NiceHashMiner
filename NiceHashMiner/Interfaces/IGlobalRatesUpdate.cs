using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces
{
    public interface IGlobalRatesUpdate
    {
        void UpdateGlobalRate();

        void StartMiningGui();
        void StopMiningGui();
    }
}
