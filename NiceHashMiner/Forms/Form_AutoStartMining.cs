using NiceHashMiner.Enums;
using NiceHashMiner.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public partial class Form_AutoStartMining : Form
    {
        public Form_AutoStartMining()
        {
            InitializeComponent();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            IMiningControl miningControl = Owner as IMiningControl;
            if (miningControl != null)
            {
                Helpers.ConsolePrint("NICEHASH", "AutoStart");

                StartMiningReturnType miningReturn = miningControl.StartMining(false);
                switch (miningReturn)
                {
                    case StartMiningReturnType.StartMining:
                        timer1.Stop();
                        Close();
                        break;
                        
                    case StartMiningReturnType.IgnoreMsgNullNiceHashData:
                        label_msg.Text = International.GetText("Form_Main_msgbox_NullNiceHashDataMsg");
                        miningControl.StopMining();
                        break;

                    case StartMiningReturnType.IgnoreMsgDemoMode:
                        label_msg.Text = International.GetText("Form_Main_DemoModeLabel");
                        miningControl.StopMining();
                        break;

                    case StartMiningReturnType.IgnoreMsgUnbenchmarkedAlgorithms:
                        label_msg.Text = International.GetText("EnabledUnbenchmarkedAlgorithmsWarning");
                        miningControl.StopMining();
                        break;

                    default:
                        miningControl.StopMining();
                        break;
                }
            }
        }

        private void Form_AutoStartMining_Shown(object sender, EventArgs e)
        {
            timer1.Start();
        }
    }
}
