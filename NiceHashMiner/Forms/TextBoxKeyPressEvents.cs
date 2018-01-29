using System.Windows.Forms;

namespace NiceHashMiner
{
    public static class TextBoxKeyPressEvents
    {
        public static void TextBoxIntsOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            // allow only one zero
            if (sender is TextBox textBox && textBox.SelectionLength != textBox.Text.Length &&
                IsHandleZero(e, textBox.Text))
            {
                e.Handled = true;
                return;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        public static void TextBoxDoubleOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            // allow only one zero
            if (sender is TextBox textBox)
            {
                var checkText = textBox.Text;
                if (e.KeyChar != '.' && textBox.SelectionLength != textBox.Text.Length && IsHandleZero(e, checkText) &&
                    !checkText.Contains("."))
                {
                    e.Handled = true;
                    return;
                }
            }
            if (DoubleInvalid(e.KeyChar))
            {
                e.Handled = true;
            }
            // only allow one decimal point
            if ((e.KeyChar == '.') && (((TextBox) sender).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private static bool DoubleInvalid(char c)
        {
            return !char.IsControl(c) && !char.IsDigit(c) && (c != '.');
        }

        private static bool IsHandleZero(KeyPressEventArgs e, string checkText)
        {
            return !char.IsControl(e.KeyChar) && checkText.Length > 0 && checkText[0] == '0';
        }
    }
}
