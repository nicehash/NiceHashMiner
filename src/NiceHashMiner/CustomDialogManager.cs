using NiceHashMiner.Views.Common.NHBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NiceHashMiner
{
    internal static class CustomDialogManager
    {
        internal static NHMMainWindow MainWindow { get; set; }
        internal static void ShowDialog(UserControl userControl)
        {
            MainWindow?.ShowContentAsModal(userControl);
        }


        internal static void HideCurrentModal()
        {
            MainWindow?.HideModal();
        }
    }
}
