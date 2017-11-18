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
    public partial class Form_Msg : Form
    {
        public Form_Msg()
        {
            InitializeComponent();
        }

        private int timeOutInSec = 0;
        private DialogResult timeOutDialogResult = DialogResult.None;
        private Button timeOutButton = null;
        private string buttonText = "";

        private void AddButton(DialogResult dialogResult)
        {
            Button button = new Button();
            button.DialogResult = dialogResult;
            button.Text = International.GetText("Global_" + dialogResult.ToString());
            button.Click += new EventHandler(button_Click);
            panelBut.Controls.Add(button);

            if(dialogResult == timeOutDialogResult)
            {
                timeOutButton = button;
                buttonText = button.Text;
            }
        }

        internal DialogResult ShowMsg(string text, string caption, MessageBoxButtons buttons, int timeout, DialogResult timeOutResult)
        {
            timeOutInSec = timeout;
            timeOutDialogResult = timeOutResult;

            label_msg.Text = text;
            Text = caption;


            DialogResult result = DialogResult.None;

            switch (buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    AddButton(DialogResult.Abort);
                    AddButton(DialogResult.Retry);
                    AddButton(DialogResult.Ignore);
                    break;

                case MessageBoxButtons.OK:
                    AddButton(DialogResult.OK);
                    break;

                case MessageBoxButtons.OKCancel:
                    AddButton(DialogResult.OK);
                    AddButton(DialogResult.Cancel);
                    break;

                case MessageBoxButtons.RetryCancel:
                    AddButton(DialogResult.Retry);
                    AddButton(DialogResult.Cancel);
                    break;

                case MessageBoxButtons.YesNo:
                    AddButton(DialogResult.Yes);
                    AddButton(DialogResult.No);
                    break;

                case MessageBoxButtons.YesNoCancel:
                    AddButton(DialogResult.Yes);
                    AddButton(DialogResult.No);
                    AddButton(DialogResult.Cancel);
                    break;

            }

            if (timeOutButton != null)
            {
                timer1_Tick(null, null);
            }

            result = ShowDialog();
            return result;
        }

        private void button_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            Close();
        }

        private void panelBut_ControlAdded(object sender, ControlEventArgs e)
        {
            int buttonsWidth = 0;
            foreach (Control con in panelBut.Controls)
            {
                Button button = con as Button;
                if (button != null)
                {
                    buttonsWidth += button.Size.Width + 10;
                }
            }
            buttonsWidth -= 10;


            int labelWidth = label_msg.Width + 10;
            Width = (labelWidth > buttonsWidth) ? labelWidth : buttonsWidth;

            int X = (/*panelBut.*/Width - buttonsWidth) / 2;

            foreach (Control con in panelBut.Controls)
            {
                Button button = con as Button;
                if (button != null)
                {
                    button.Location = new Point(X, (panelBut.Height - button.Height) / 2);
                    X += button.Width + 10;
                }
            }
        }

        private void Form_Msg_Shown(object sender, EventArgs e)
        {
            if (timeOutInSec == 0)
            {
                timer1.Stop();
            }
            else
            {
                timer1.Interval = 1000;
                timer1.Start();
            }

            Height = label_msg.Height + 0 + panelBut.Height;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timeOutButton.Text = buttonText + "("+timeOutInSec-- +")";
            if(timeOutInSec < 0)
            {
                timer1.Stop();
                this.DialogResult = timeOutDialogResult;
                Close();
            }
        }
    }
}
