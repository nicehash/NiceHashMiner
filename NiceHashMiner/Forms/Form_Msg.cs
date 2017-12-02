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
            //button.DialogResult = dialogResult;
            button.Text = dialogResult.ToString();
            button.Size = new Size(80, 27);
            button.Margin = new Padding(5);
            button.Click += new EventHandler(button_Click);
            flowLayoutPanel2.Controls.Add(button);

            if (dialogResult == timeOutDialogResult)
            {
                timeOutButton = button;
                buttonText = button.Text;
            }
        }

        private Icon GetIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Information:
                    return SystemIcons.Information;
                case MessageBoxIcon.Question:
                    return SystemIcons.Question;
                case MessageBoxIcon.Error:
                    return SystemIcons.Error;
                case MessageBoxIcon.Exclamation:
                    return SystemIcons.Exclamation;
                default:
                    return null;
            }
        }

        internal DialogResult ShowMsg(string text, string caption, MessageBoxButtons buttons, int timeout, DialogResult timeOutResult, MessageBoxIcon icon)
        {
            timeOutInSec = timeout;
            timeOutDialogResult = timeOutResult;

            DialogResult result = DialogResult.None;

            label_msg.Text = caption;
            Width = label_msg.Width;

            if (icon != MessageBoxIcon.None)
            {
                pictureBox1.Image = new Icon(GetIcon(icon), 32, 32).ToBitmap();
            }
            else
            {
                pictureBox1.Visible = false;
            }

            switch (buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    AddButton(DialogResult.Ignore);
                    AddButton(DialogResult.Retry);
                    AddButton(DialogResult.Abort);
                    break;

                case MessageBoxButtons.OK:
                    AddButton(DialogResult.OK);
                    break;

                case MessageBoxButtons.OKCancel:
                    AddButton(DialogResult.Cancel);
                    AddButton(DialogResult.OK);
                    break;

                case MessageBoxButtons.RetryCancel:
                    AddButton(DialogResult.Cancel);
                    AddButton(DialogResult.Retry);
                    break;

                case MessageBoxButtons.YesNo:
                    AddButton(DialogResult.No);
                    AddButton(DialogResult.Yes);
                    break;

                case MessageBoxButtons.YesNoCancel:
                    AddButton(DialogResult.Cancel);
                    AddButton(DialogResult.No);
                    AddButton(DialogResult.Yes);
                    break;

            }

            label_msg.Text = text;
            Text = caption;

            if (Width < flowLayoutPanel3.Width)
            {
                Width = flowLayoutPanel3.Width + flowLayoutPanel3.Margin.Right;
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
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timeOutButton.Text = buttonText + "(" + timeOutInSec-- + ")";
            if (timeOutInSec < 0)
            {
                timer1.Stop();
                this.DialogResult = timeOutDialogResult;
                Close();
            }
        }


    }
}
