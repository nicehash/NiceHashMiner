using System.Windows;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for _3rdPartyTosWindow.xaml
    /// </summary>
    public partial class EulaWindowSecondShort : BaseDialogWindow
    {
        public bool Accepted { get; private set; } = false;

        public EulaWindowSecondShort()
        {
            InitializeComponent();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
