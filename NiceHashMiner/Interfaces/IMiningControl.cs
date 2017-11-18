using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces
{
    interface IMiningControl
    {
        Enums.StartMiningReturnType StartMining(bool showWarnings);
        void StopMining();
    }
}
