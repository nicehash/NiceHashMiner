using NiceHashMiner.Interfaces.DataVisualizer;
using NiceHashMiner.Interfaces.StateSetters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public static class FormHelpers
    {

        public static void TranslateFormControls(Control c)
        {
            var name = "";
            var trText = "";
            try
            {
                name = c.Name;
                var fromTxt = c.Text;
                trText = Translations.Tr(fromTxt);
                c.Text = trText;

                //Helpers.ConsolePrint("FormHelpers.TranslateFormControls", $"ControlName: {name}, fromText: {fromTxt}, toText: {trText}");
            }
            catch(Exception e)
            {
                //Helpers.ConsolePrint("FormHelpers.TranslateFormControls", $"Cannot translate ControlName: {name}, exception: {e}");
            }

            // special cases
            if (c is ComboBox comboBox)
            {

                for (var i = 0; i < comboBox.Items.Count; i++)
                {
                    if (comboBox.Items[i] is string text)
                    {
                        comboBox.Items[i] = Translations.Tr(text);
                    }
                }
            }
            if (c is ListView listView)
            {
                for (var i = 0; i < listView.Columns.Count; i++)
                {
                    listView.Columns[i].Text = Translations.Tr(listView.Columns[i].Text);
                }
            }
            if (c is TabControl tabControl)
            {
                for (var i = 0; i < tabControl.TabPages.Count; i++)
                {
                    tabControl.TabPages[i].Text = Translations.Tr(tabControl.TabPages[i].Text);
                }
            }

            // call on all controls
            foreach (Control childC in c.Controls)
            {
                TranslateFormControls(childC);
            }
        }

        static public void SafeInvoke(this Control c, Action f, bool beginInvoke = false)
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
