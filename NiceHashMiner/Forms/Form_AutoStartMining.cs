using NiceHashMiner.Enums;
using NiceHashMiner.Interfaces;
using System;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public partial class Form_AutoStartMining : Form
    {
        public Form_AutoStartMining()
        {
            InitializeComponent();
        }

        private IMiningControl miningControl;

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            Close();
        }

      

        private void Form_AutoStartMining_Shown(object sender, EventArgs e)
        {
            miningControl = Owner as IMiningControl;
            timer1.Interval = 5000;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (miningControl == null)
            {
                Close();
            }
            else
            {
                Helpers.ConsolePrint("NICEHASH", "AutoStart");

                StartMiningReturnType miningReturn = miningControl.StartMining(true);
                if(miningReturn == StartMiningReturnType.StartMining)
                {
                    Close();
                }
                else
                {
                    miningControl.StopMining();
                    timer1.Start();
                }
            }
        }
    }
}
