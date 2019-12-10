using System.Windows;
using NHM.Wpf.Views.Common;
using NHM.Wpf.Views.Common.NHBase;

namespace NHM.Wpf.Views
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
