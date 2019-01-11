using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        delegate bool InitStep();
        internal class ActionWithMessage
        {
            InitStep initStep;
            string message;
        }

        // TODO add init stuff here
        public static void InitializeManagersAndMiners()
        {
            var initSteps = new List<ActionWithMessage>();
        }

        
    }
}
