// SHARED
using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Miners;
using NiceHashMiner.Stats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static NiceHashMiner.Translations;

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

        public static void ShowQueryWarnings(QueryResult query)
        {
            if (query.FailedMinNVDriver)
            {
                MessageBox.Show(string.Format(
                        Tr(
                            "We have detected that your system has Nvidia GPUs, but your driver is older than {0}. In order for NiceHash Miner Legacy to work correctly you should upgrade your drivers to recommended {1} or newer. If you still see this warning after updating the driver please uninstall all your Nvidia drivers and make a clean install of the latest official driver from http://www.nvidia.com."),
                        query.MinDriverString,
                        query.RecommendedDriverString),
                    Tr("Nvidia Recomended driver"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (query.FailedRecommendedNVDriver)
            {
                MessageBox.Show(string.Format(
                        Tr(
                            "We have detected that your Nvidia Driver is older than {0}{1}. We recommend you to update to {2} or newer."),
                        query.RecommendedDriverString,
                        query.CurrentDriverString,
                        query.RecommendedDriverString),
                    Tr("Nvidia Recomended driver"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (query.NoDevices)
            {
                var result = MessageBox.Show(Tr("No supported devices are found. Select the OK button for help or cancel to continue."),
                    Tr("No Supported Devices"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    Process.Start(Links.NhmNoDevHelp);
                }
            }

            if (query.FailedRamCheck)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy recommends increasing virtual memory size so that all algorithms would work fine."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);
            }

            if (query.FailedVidControllerStatus)
            {
                var msg = Tr("We have detected a Video Controller that is not working properly. NiceHash Miner Legacy will not be able to use this Video Controller for mining. We advise you to restart your computer, or reinstall your Video Controller drivers.");
                msg += '\n' + query.FailedVidControllerInfo;
                MessageBox.Show(msg,
                    Tr("Warning! Video Controller not operating correctly"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            //if (query.FailedCpuCount)
            //{
            //    MessageBox.Show(Tr("NiceHash Miner Legacy does not support more than 64 virtual cores. CPU mining will be disabled."),
            //        Tr("Warning!"),
            //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }

        private static bool isInitFinished = false;
        public static void InitFinished()
        {
            isInitFinished = true;
            // TESTNET
#if TESTNET || TESTNETDEV
            NiceHashStats.StateChanged();
#endif
        }
    }
}
