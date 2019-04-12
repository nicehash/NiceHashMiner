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
    public static partial class FormHelpers
    {
        
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
