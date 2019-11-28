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

namespace NHM.Wpf.Views.Benchmark.ComputeDeviceItem
{
    /// <summary>
    /// Interaction logic for AlgorithmItem.xaml
    /// </summary>
    public partial class AlgorithmItem : UserControl
    {
        public AlgorithmItem()
        {
            InitializeComponent();

            DataContextChanged += AlgorithmItem_DataContextChanged;
        }

        private void AlgorithmItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void AlgorithmSettings_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
