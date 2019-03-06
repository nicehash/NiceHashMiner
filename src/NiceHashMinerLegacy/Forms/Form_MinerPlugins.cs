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
    public partial class Form_MinerPlugins : Form
    {
        public Form_MinerPlugins()
        {
            InitializeComponent();
            CenterToScreen();
            Icon = Properties.Resources.logo;
            FormHelpers.TranslateFormControls(this);
        }
    }
}
