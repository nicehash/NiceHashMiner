using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace NHM.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for StartupLoadingWindow.xaml
    /// </summary>
    public partial class StartupLoadingWindow : Window
    {
        public bool CanClose { get; set; } = true;

        public StartupLoadingWindow()
        {
            InitializeComponent();
        }

        private void StartupLoadingWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!CanClose) e.Cancel = true;
        }
    }
}
