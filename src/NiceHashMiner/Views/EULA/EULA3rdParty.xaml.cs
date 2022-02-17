using NHMCore;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
