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
        private int trynumber = 1;
        private int waitSeconds = 25;
        private bool isFirst = true;

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            Close();
        }      

        private void Form_AutoStartMining_Shown(object sender, EventArgs e)
        {
            miningControl = Owner as IMiningControl;
            Text = $"AutoStart wait {waitSeconds--}s";
            timer1.Interval = 1000;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isFirst)
            {
                Text = $"AutoStart wait {waitSeconds--}s";
                if (waitSeconds < 0)
                {
                    isFirst = false;
                    timer1.Interval = 5000;
                }
                else
                {
                    return;
                }
            }

            timer1.Stop();
            if (miningControl == null)
            {
                Close();
            }
            else
            {
                Text = $"AutoStart try {trynumber++}";
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

        private void Form_AutoStartMining_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
        }
    }
}
