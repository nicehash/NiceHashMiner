using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    static class FormHelpers
    {
        // TODO maybe not the best name
        static public void SafeInvoke(Control c, MethodInvoker f)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(f);
            }
            else
            {
                f();
            }
        }
    }
}
