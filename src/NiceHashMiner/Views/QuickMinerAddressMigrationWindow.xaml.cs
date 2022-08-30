using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for QuickMinerAddressMigrationWindow.xaml
    /// </summary>
    public partial class QuickMinerAddressMigrationWindow : BaseDialogWindow
    {
        public QuickMinerAddressMigrationWindow()
        {
            InitializeComponent();
            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            BTC.Text = CredentialsSettings.Instance.QuickMinerMiningAddress;
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            CustomDialogManager.HideCurrentModal();
        }

        private void ConfirmButtonClicked(object sender, RoutedEventArgs e)
        {
            CredentialsSettings.Instance.SetBitcoinAddress(CredentialsSettings.Instance.QuickMinerMiningAddress);
            Close();
        }

        private void NewMiningAddressClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
