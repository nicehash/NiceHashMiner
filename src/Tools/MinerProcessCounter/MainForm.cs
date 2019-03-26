using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinerProcessCounter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.Shown += new System.EventHandler(this.Form_Main_Shown);
        }

        private void Form_Main_Shown(object sender, EventArgs e)
        {
            var psSnapshotIntervalTimer = new Timer();
            psSnapshotIntervalTimer.Tick += StartupTimer_Tick;
            psSnapshotIntervalTimer.Interval = 500;
            psSnapshotIntervalTimer.Start();
        }

        private void StartupTimer_Tick(object sender, EventArgs e)
        {
            var running = ProcessWalker.ListRunning();
            dataGridView1.Rows.Clear();
            foreach (var r in running)
            {
                object[] rowData = { r.ProcessName, r.FileName, r.Arguments };
                dataGridView1.Rows.Add(rowData);
            }
        }
    }
}
