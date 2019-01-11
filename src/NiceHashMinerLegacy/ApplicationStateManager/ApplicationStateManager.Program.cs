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

        // EnsureSystemRequirements will check if all system requirements are met if not it will show a error/warning message box and exit the application
        // TODO this one holds
        public static bool SystemRequirementsEnsured()
        {
            // check WMI
            if (!Helpers.IsWmiEnabled())
            {
                MessageBox.Show(International.GetText("Program_WMI_Error_Text"),
                        International.GetText("Program_WMI_Error_Title"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }

            if (!Helpers.Is45NetOrHigher())
            {
                MessageBox.Show(International.GetText("NET45_Not_Installed_msg"),
                    International.GetText("Warning_with_Exclamation"),
                    MessageBoxButtons.OK);

                return false;
            }

            if (!Helpers.Is64BitOperatingSystem)
            {
                MessageBox.Show(International.GetText("Form_Main_x64_Support_Only"),
                    International.GetText("Warning_with_Exclamation"),
                    MessageBoxButtons.OK);

                return false;
            }

            return true;
        }
    }
}
