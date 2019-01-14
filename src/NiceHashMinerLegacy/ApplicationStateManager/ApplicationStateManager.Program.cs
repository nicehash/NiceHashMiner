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
                MessageBox.Show(Translations.Tr("NiceHash Miner Legacy cannot run needed components. It seems that your system has Windows Management Instrumentation service Disabled. In order for NiceHash Miner Legacy to work properly Windows Management Instrumentation service needs to be Enabled. This service is needed to detect RAM usage and Avaliable Video controler information. Enable Windows Management Instrumentation service manually and start NiceHash Miner Legacy."),
                        Translations.Tr("Windows Management Instrumentation Error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }

            if (!Helpers.Is45NetOrHigher())
            {
                MessageBox.Show(Translations.Tr("NiceHash Miner Legacy requires .NET Framework 4.5 or higher to work properly. Please install Microsoft .NET Framework 4.5"),
                    Translations.Tr("Warning!"),
                    MessageBoxButtons.OK);

                return false;
            }

            if (!Helpers.Is64BitOperatingSystem)
            {
                MessageBox.Show(Translations.Tr("NiceHash Miner Legacy supports only x64 platforms. You will not be able to use NiceHash Miner Legacy with x86"),
                    Translations.Tr("Warning!"),
                    MessageBoxButtons.OK);

                return false;
            }

            return true;
        }
    }
}
