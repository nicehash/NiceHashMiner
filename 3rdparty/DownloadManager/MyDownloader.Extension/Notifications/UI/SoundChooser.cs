using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MyDownloader.Extension.Notifications.Helpers;

namespace MyDownloader.Extension.Notifications.UI
{
    public partial class SoundChooser : UserControl
    {
        public SoundChooser()
        {
            InitializeComponent();
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                return lblText.Text;
            }
            set
            {
                lblText.Text = value;
            }
        }

        public string FileName
        {
            get
            {
                return txtSound.Text;
            }
            set
            {
                txtSound.Text = value;
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (openFDlg.ShowDialog() == DialogResult.OK)
            {
                txtSound.Text = openFDlg.FileName;
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            SoundHelper.PlayWav(txtSound.Text);
        }
    }
}
