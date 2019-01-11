using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        public static void RestartProgram()
        {
            var pHandle = new Process
            {
                StartInfo =
                                {
                                    FileName = Application.ExecutablePath
                                }
            };
            pHandle.Start();
            Application.Exit();
        }
    }
}
