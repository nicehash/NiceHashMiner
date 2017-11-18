using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Utils
{
    class MsgBox
    {
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            return Show(text, caption, buttons, 0, DialogResult.None);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, int timeout, DialogResult timeOutResult)
        {
            DialogResult result = DialogResult.None;
            using (Forms.Form_Msg msgBox = new Forms.Form_Msg())
            {
                result = msgBox.ShowMsg(text, caption, buttons, timeout, timeOutResult);
            }
            return result;
        }

        internal static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, int timeout, DialogResult timeOutResult)
        {
            return Show(text, caption, buttons, timeout, timeOutResult);
        }

        internal static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(text, caption, buttons, 0, DialogResult.None);
        }
    }
}
