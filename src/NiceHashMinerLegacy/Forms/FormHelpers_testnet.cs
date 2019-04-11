// TESTNET
#if TESTNET || TESTNETDEV

using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Interfaces.StateSetters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    static class FormHelpers
    {
        // TODO maybe not the best name
        static public void SafeInvoke(Control c, Action f, bool beginInvoke = false)
        {
            if (c.InvokeRequired)
            {
                if (beginInvoke)
                {
                    c.BeginInvoke(f);
                }
                else
                {
                    c.Invoke(f);
                }
            }
            else
            {
                f();
            }
        }

        static public void SubscribeAllControls(Control c)
        {
            // data display
            if (c is IDataVisualizer dv)
            {
                ApplicationStateManager.SubscribeStateDisplayer(dv);
            }
            // setters
            if (c is IEnabledDeviceStateSetter setter)
            {
                setter.SetDeviceEnabledState += ApplicationStateManager.SetDeviceEnabledState;
            }
            // call on all controls
            foreach (Control childC in c.Controls)
            {
                SubscribeAllControls(childC);
            }
        }

        static public void UnsubscribeAllControls(Control c)
        {
            if (c is IDataVisualizer dv)
            {
                ApplicationStateManager.UnsubscribeStateDisplayer(dv);
            }
            // setters
            if (c is IEnabledDeviceStateSetter setter)
            {
                setter.SetDeviceEnabledState -= ApplicationStateManager.SetDeviceEnabledState;
            }
            // call on all controls
            foreach (Control childC in c.Controls)
            {
                SubscribeAllControls(childC);
            }
        }
    }
}
#endif
