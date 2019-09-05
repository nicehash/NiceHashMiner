using System.Windows;
using NHM.Wpf.Views.Common;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for _3rdPartyTosWindow.xaml
    /// </summary>
    public partial class _3rdPartyTosWindow : Window
    {
        public _3rdPartyTosWindow()
        {
            InitializeComponent();

            WindowUtils.Translate(this);
        }

        private void AgreeButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void RefuseButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
