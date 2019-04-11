// TESTNET
#if TESTNET || TESTNETDEV
﻿using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        delegate void InitStep();
        class ActionWithMessage
        {
            public InitStep initStep { get; set; }
            public string message { get; set; }
        }

        // TODO add init stuff here
        public static async void InitializeManagersAndMiners()
        {
            var initSteps = new List<ActionWithMessage>();
        }

        private static bool isInitFinished = false;
        public static void InitFinished()
        {
            isInitFinished = true;
            NiceHashStats.StateChanged();
        }
    }
}
#endif
