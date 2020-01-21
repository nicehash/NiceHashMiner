using NHM.Common;
using NiceHashMiner.ViewModels;
using NiceHashMiner.Views.Common;
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

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for TemporaryStartupLoadingControl.xaml
    /// </summary>
    public partial class TemporaryStartupLoadingControl : UserControl
    {
        public IStartupLoader StartupLoader { get; }
        public TemporaryStartupLoadingControl()
        {
            InitializeComponent();
            StartupLoader = this.AssertViewModel<StartupLoadingVM>();
        }
    }
}
