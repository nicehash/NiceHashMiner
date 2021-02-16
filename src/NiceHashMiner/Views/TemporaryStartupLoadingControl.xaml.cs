using NHM.Common;
using NiceHashMiner.ViewModels;
using NiceHashMiner.Views.Common;
using System.Windows.Controls;

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
