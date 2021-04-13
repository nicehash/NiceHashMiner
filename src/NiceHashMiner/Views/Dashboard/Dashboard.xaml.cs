using NHM.Common;
using NHMCore.ApplicationState;
using NiceHashMiner.ViewModels;
using NiceHashMiner.Views.Common;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.Dashboard
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl, IThemeSetter
    {
        private MainVM _vm;

        public Dashboard()
        {
            InitializeComponent();

            DataContextChanged += Dashboard_DataContextChanged;
            ThemeSetterManager.AddThemeSetter(this);
        }

        private void Dashboard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MainVM mainVM)
            {
                _vm = mainVM;
                return;
            }
            throw new Exception("Dashboard_DataContextChanged e.NewValue must be of type MainVM");
        }

        void IThemeSetter.SetTheme(bool isLight)
        {
            var style = isLight ? Application.Current.FindResource("StartStopButtonLight") : Application.Current.FindResource("StartStopButtonDark");
            StartStopToggleButton.Style = style as Style;
        }

        private async void ToggleButtonStartStop_Click(object sender, RoutedEventArgs e)
        {
            await ToggleButtonStartStop_ClickTask();
        }

        private async Task ToggleButtonStartStop_ClickTask()
        {
            try
            {
                StartStopToggleButton.IsEnabled = false;
                // IF ANY MINING execute STOP
                if (MiningState.Instance.AnyDeviceRunning)
                {
                    await _vm.StopMining();
                }
                else
                {
                    await _vm.StartMining();
                }
            }
            catch (Exception e)
            {
                Logger.Error("NiceHashMiner.Views.Dashboard", $"ToggleButtonStartStop_ClickTask Error: {e}");
            }
            finally
            {
                StartStopToggleButton.IsEnabled = true;
            }
        }


        private static readonly EnterWalletDialog _enterBTCAddress = new EnterWalletDialog();
        private void EnterBTCWallet_Button_Click(object sender, RoutedEventArgs e)
        {
            CustomDialogManager.ShowDialog(_enterBTCAddress);
        }
    }
}
