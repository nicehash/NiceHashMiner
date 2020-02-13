using NiceHashMiner.Views.Common.NHBase;
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

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for SingleInstanceNotice.xaml
    /// </summary>
    public partial class SingleInstanceNotice : BaseDialogWindow
    {
        public SingleInstanceNotice()
        {
            InitializeComponent();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
