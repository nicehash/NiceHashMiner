using System;
using System.Drawing;
using System.Windows.Forms;
using NHM.Common;

namespace NiceHashMiner.Forms.Components
{
    public partial class StartupLoadingControl : UserControl, IStartupLoader
    {
        public StartupLoadingControl(string title = null)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(title)) PrimaryTitle = title;
            SecondaryVisible = false;
            label_LoadingTitle.Location = new Point((Size.Width - label_LoadingTitle.Size.Width) / 2,
                label_LoadingTitle.Location.Y);

            PrimaryProgress = new Progress<(string loadMessageText, int perc)>(p =>
            {
                Progress = p.perc;
                LoadMessageText = p.loadMessageText;
            });

            SecondaryProgress = new Progress<(string loadMessageText, int perc)>(p =>
            {
                ProgressSecond = p.perc;
                LoadMessageTextSecond = p.loadMessageText;
            });

            FormHelpers.TranslateFormControls(this);
        }

        public string PrimaryTitle
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
        public bool SecondaryVisible
        {
            get
            {
                return _secondProgressVisible;
            }
            set
            {
                if (_secondProgressVisible == value) return;
                _secondProgressVisible = value;
                Height = _secondProgressVisible ? _visibleHeight : _hiddenHeight;
            }
        }


        public string SecondaryTitle
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

        public IProgress<(string, int)> PrimaryProgress { get; private set; }

        public IProgress<(string, int)> SecondaryProgress { get; private set; }
    }
}
