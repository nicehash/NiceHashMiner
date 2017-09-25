using System.Windows.Forms;

namespace NiceHashMiner.Interfaces
{
    /// <summary>
    /// IMessageNotifier interface is for message setting.
    /// </summary>
    public interface IMessageNotifier
    {
        void SetMessage(string infoMsg);
        void SetMessageAndIncrementStep(string infoMsg);
        DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}
