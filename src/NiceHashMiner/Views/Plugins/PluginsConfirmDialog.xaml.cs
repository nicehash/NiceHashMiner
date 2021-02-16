using NHMCore;
using NHMCore.Mining.Plugins;
using NiceHashMiner.Views.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static NHMCore.Translations;

namespace NiceHashMiner.Views.Plugins
{
    /// <summary>
    /// Interaction logic for PluginsConfirmDialog.xaml
    /// </summary>
    public partial class PluginsConfirmDialog : UserControl
    {
        public class VM
        {
            public ObservableCollection<PluginPackageInfoCR> Plugins { get; set; }
        }
        int _pluginsToAccept = 0;
        public PluginsConfirmDialog()
        {
            InitializeComponent();

            this.DataContextChanged += PluginsConfirmDialog_DataContextChanged;
        }

        private void PluginsConfirmDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as VM;
            _pluginsToAccept = vm?.Plugins?.Count() ?? 0;
            if (_pluginsToAccept == 0) CustomDialogManager.HideCurrentModal();
        }

        private void OnAcceptOrDecline(object sender, RoutedEventArgs e)
        {
            _pluginsToAccept--;
            if (_pluginsToAccept == 0)
            {
                CustomDialogManager.HideCurrentModal();
                var nhmRestartDialog = new CustomDialog()
                {
                    Title = Tr("Restart NiceHash Miner"),
                    Description = Tr("NiceHash Miner restart is required."),
                    OkText = Tr("Restart"),
                    AnimationVisible = Visibility.Collapsed,
                    CancelVisible = Visibility.Collapsed,
                    ExitVisible = Visibility.Collapsed,
                };
                nhmRestartDialog.OKClick += (s, e1) =>
                {
                    Task.Run(() => ApplicationStateManager.RestartProgram());
                };
                CustomDialogManager.ShowModalDialog(nhmRestartDialog);

                var nhmRestartingDialog = new CustomDialog()
                {
                    Title = Tr("NiceHash Miner Restarting"),
                    Description = Tr("NiceHash Miner restart in progress."),
                    CancelVisible = Visibility.Collapsed,
                    OkVisible = Visibility.Collapsed,
                    AnimationVisible = Visibility.Visible,
                    ExitVisible = Visibility.Collapsed
                };
                CustomDialogManager.ShowModalDialog(nhmRestartingDialog);
            }
        }


    }
}
