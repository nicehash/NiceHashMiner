using NiceHashMiner.Views.Common.NHBase;
using System.Windows.Controls;

namespace NiceHashMiner
{
    internal static class CustomDialogManager
    {
        internal static NHMMainWindow MainWindow { get; set; }
        internal static void ShowDialog(UserControl userControl)
        {
            MainWindow?.ShowContentAsDialog(userControl);
        }

        internal static void ShowModalDialog(UserControl userControl)
        {
            MainWindow?.ShowContentAsModalDialog(userControl);
        }


        internal static void HideCurrentModal()
        {
            MainWindow?.HideModal();
        }
    }
}
