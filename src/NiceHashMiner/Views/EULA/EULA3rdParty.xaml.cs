using NHMCore;
using NHMCore.Configs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.EULA
{
    /// <summary>
    /// Interaction logic for EULA3rdParty.xaml
    /// </summary>
    public partial class EULA3rdParty : UserControl
    {
        public event EventHandler<RoutedEventArgs> OKClick;
        public EULA3rdParty()
        {
            InitializeComponent();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToSSetings.Instance.Use3rdPartyMinersTOS = ApplicationStateManager.CurrentTosVer;
            this.Visibility = Visibility.Hidden;
            OKClick?.Invoke(this, e);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationStateManager.ExecuteApplicationExit();
        }
    }
}
