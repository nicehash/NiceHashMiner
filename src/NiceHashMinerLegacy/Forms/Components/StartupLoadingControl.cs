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
    public partial class StartupLoadingControl : UserControl
    {
        public StartupLoadingControl(string title = null)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(title)) LoadTitleText = title;
            ShowSecondProgressBar = false;
            label_LoadingTitle.Location = new Point((Size.Width - label_LoadingTitle.Size.Width) / 2,
                label_LoadingTitle.Location.Y);

            FormHelpers.TranslateFormControls(this);
        }

        public string LoadTitleText
        {
            get
            {
                return label_LoadingTitle.Text;
            }
            set
            {
                label_LoadingTitle.Text = value;
            }
        }

        public int Progress
        {
            get
            {
                return progressBar1.Value;
            }
            set
            {
                progressBar1.Value = value;
                progressBar1.Invalidate();
            }
        }

        public string LoadMessageText
        {
            get
            {
                return label_LoadStepMessageText.Text;
            }
            set
            {
                label_LoadStepMessageText.Text = value;
                label_LoadStepMessageText.Invalidate();
            }
        }


        bool _secondProgressVisible = true;
        const int _hiddenHeight = 76;
        const int _visibleHeight = 146;
        public bool ShowSecondProgressBar
        {
            get
            {
                return _secondProgressVisible;
            }
            set
            {
                if (_secondProgressVisible == value) return;
                _secondProgressVisible = value;
                this.Height = _secondProgressVisible ? _visibleHeight : _hiddenHeight;
            }
        }


        public string LoadTitleTextSecond
        {
            get
            {
                return label_Title2.Text;
            }
            set
            {
                label_Title2.Text = value;
            }
        }

        public int ProgressSecond
        {
            get
            {
                return progressBar2.Value;
            }
            set
            {
                progressBar2.Value = value;
                progressBar2.Invalidate();
            }
        }

        public string LoadMessageTextSecond
        {
            get
            {
                return label_LoadStepMessageText2.Text;
            }
            set
            {
                label_LoadStepMessageText2.Text = value;
                label_LoadStepMessageText2.Invalidate();
            }
        }
    }
}
