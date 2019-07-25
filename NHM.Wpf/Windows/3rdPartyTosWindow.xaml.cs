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
using System.Windows.Shapes;
using NHM.Wpf.Windows.Common;

namespace NHM.Wpf.Windows
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
