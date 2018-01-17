using System;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    public partial class Field : UserControl
    {
        public string LabelText
        {
            get => labelFieldIndicator.Text;
            set
            {
                if (value != null)
                {
                    labelFieldIndicator.Text = value;
                }
            }
        }

        public string EntryText
        {
            get => textBox.Text;
            set
            {
                if (value != null)
                {
                    textBox.Text = value;
                }
            }
        }

        public void InitLocale(ToolTip toolTip1, string infoLabel, string infoMsg)
        {
            labelFieldIndicator.Text = infoLabel;
            toolTip1.SetToolTip(labelFieldIndicator, infoMsg);
            toolTip1.SetToolTip(textBox, infoMsg);
            toolTip1.SetToolTip(pictureBox1, infoMsg);
        }

        public void SetInputModeDoubleOnly()
        {
            textBox.KeyPress += TextBoxKeyPressEvents.TextBoxDoubleOnly_KeyPress;
        }

        public void SetInputModeIntOnly()
        {
            textBox.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
        }

        public void SetOnTextChanged(EventHandler textChanged)
        {
            textBox.TextChanged += textChanged;
        }

        public void SetOnTextLeave(EventHandler textLeave)
        {
            textBox.Leave += textLeave;
        }

        public Field()
        {
            InitializeComponent();
        }
    }
}
