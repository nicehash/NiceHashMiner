using NiceHashMiner.Forms;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NiceHashMiner
{
    public partial class Form_Loading : Form, IMinerUpdateIndicator
    {
        public interface IAfterInitializationCaller
        {
            void AfterLoadComplete();
        }

        private int _loadCounter = 0;
        private readonly int _totalLoadSteps = 12;
        private readonly IAfterInitializationCaller _afterInitCaller;

        private readonly MinersDownloader _minersDownloader = null;

        // init loading stuff
        public Form_Loading(IAfterInitializationCaller initCaller, string loadFormTitle, string startInfoMsg,
            int totalLoadSteps)
        {
            InitializeComponent();

            label_LoadingText.Text = loadFormTitle;
            label_LoadingText.Location = new Point((Size.Width - label_LoadingText.Size.Width) / 2,
                label_LoadingText.Location.Y);

            _afterInitCaller = initCaller;

            _totalLoadSteps = totalLoadSteps;
            progressBar1.Maximum = _totalLoadSteps;
            progressBar1.Value = 0;

            SetInfoMsg(startInfoMsg);
            FormHelpers.TranslateFormControls(this);
        }

        // download miners constructor
        public Form_Loading(MinersDownloader minersDownloader)
        {
            InitializeComponent();
            label_LoadingText.Location = new Point((Size.Width - label_LoadingText.Size.Width) / 2,
                label_LoadingText.Location.Y);
            _minersDownloader = minersDownloader;
        }

        public void IncreaseLoadCounterAndMessage(string infoMsg)
        {
            SetInfoMsg(infoMsg);
            IncreaseLoadCounter();
        }

        public void SetProgressMaxValue(int maxValue)
        {
            progressBar1.Maximum = maxValue;
        }

        public void SetInfoMsg(string infoMsg)
        {
            LoadText.Text = infoMsg;
        }

        public void IncreaseLoadCounter()
        {
            _loadCounter++;
            progressBar1.Value = _loadCounter;
            Update();
            if (_loadCounter >= _totalLoadSteps)
            {
                _afterInitCaller.AfterLoadComplete();
                Close();
                Dispose();
            }
        }

        public void FinishLoad()
        {
            while (_loadCounter < _totalLoadSteps)
            {
                IncreaseLoadCounter();
            }
        }

        public void SetValueAndMsg(int setValue, string infoMsg)
        {
            SetInfoMsg(infoMsg);
            progressBar1.Value = setValue;
            Update();
            if (progressBar1.Value >= progressBar1.Maximum)
            {
                Close();
                Dispose();
            }
        }

        #region IMessageNotifier

        public void SetMessage(string infoMsg)
        {
            SetInfoMsg(infoMsg);
        }

        public void SetMessageAndIncrementStep(object sender, string infoMsg)
        {
            IncreaseLoadCounterAndMessage(infoMsg);
        }

        #endregion //IMessageNotifier

        #region IMinerUpdateIndicator

        public void SetMaxProgressValue(int max)
        {
            Invoke((MethodInvoker) delegate
            {
                progressBar1.Maximum = max;
                progressBar1.Value = 0;
            });
        }

        public void SetProgressValueAndMsg(int value, string msg)
        {
            if (value <= progressBar1.Maximum)
            {
                Invoke((MethodInvoker) delegate
                {
                    progressBar1.Value = value;
                    LoadText.Text = msg;
                    progressBar1.Invalidate();
                    LoadText.Invalidate();
                });
            }
        }

        public void SetTitle(string title)
        {
            Invoke((MethodInvoker) delegate { label_LoadingText.Text = title; });
        }

        public void FinishMsg(bool ok)
        {
            Invoke((MethodInvoker) delegate
            {
                if (ok)
                {
                    label_LoadingText.Text = "Init Finished!";
                }
                else
                {
                    label_LoadingText.Text = "Init Failed!";
                }

                System.Threading.Thread.Sleep(1000);
                Close();
            });
        }

        #endregion IMinerUpdateIndicator


        private void Form_Loading_Shown(object sender, EventArgs e)
        {
            _minersDownloader?.Start(this);
        }
    }
}
