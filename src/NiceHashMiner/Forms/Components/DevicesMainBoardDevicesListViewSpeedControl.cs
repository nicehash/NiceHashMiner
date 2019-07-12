using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    public partial class DevicesMainBoardDevicesListViewSpeedControl : UserControl
    {
        public DevicesListViewSpeedControl SpeedsControl { get; private set; }
        public DevicesMainBoardDevicesListViewSpeedControl()
        {
            InitializeComponent();
            devicesListViewSpeedControl1.SetIsMining(true); // always is mining
            devicesListViewSpeedControl1.SaveToGeneralConfig = false;
            SpeedsControl = this.devicesListViewSpeedControl1;
            SecondPanelVisible = false;
        }

        public bool SecondPanelVisible
        {
            get
            {
                return splitContainer1.Panel2Collapsed;
            }
            set
            {
                if (value)
                {
                    splitContainer1.Panel2Collapsed = false;
                    splitContainer1.Panel2.Show();
                }
                else
                {
                    splitContainer1.Panel2Collapsed = true;
                    splitContainer1.Panel2.Hide();
                }
            }
        }
    }
}
