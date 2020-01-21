using NHMCore;
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

namespace NiceHashMiner.Views.Common
{
    /// <summary>
    /// Interaction logic for ViewStatsOnline.xaml
    /// </summary>
    public partial class ViewStatsOnline : UserControl
    {
        public ViewStatsOnline()
        {
            InitializeComponent();
        }

        private void Click_VisitStatsOnline(object sender, RoutedEventArgs e)
        {
            ApplicationStateManager.VisitMiningStatsPage();
        }
    }
}
