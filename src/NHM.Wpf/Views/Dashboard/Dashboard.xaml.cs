using NHM.Wpf.ViewModels;
using NHMCore;
using NHMCore.ApplicationState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace NHM.Wpf.Views.Dashboard
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

        private readonly HashSet<Button> _toggleButtonsGuard = new HashSet<Button>();
        private void EnterBTCWallet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                _toggleButtonsGuard.Add(tButton);
                BTCWalletContextMenu.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) => {
                    _toggleButtonsGuard.Remove(tButton);
                    //tButton.IsChecked = false;
                    BTCWalletContextMenu.Closed -= closedHandler;
                };
                BTCWalletContextMenu.Closed += closedHandler;
            }
        }

        private void AddressHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private async void TextBoxBitcoinAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBoxBTCAddress = sender as TextBox;
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            if (ApplicationStateManager.SetResult.INVALID == result)
            {
                textBoxBTCAddress.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("Red");
                //errorProvider1.SetError(textBoxBTCAddress, Tr("Invalid Bitcoin address! {0} will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!", NHMProductInfo.Name));
            }
            else if (ApplicationStateManager.SetResult.CHANGED == result)
            {
                BTCWalletContextMenu.IsOpen = false;
            }
            else
            {
                //errorProvider1.SetError(textBoxBTCAddress, "");
            }
        }
    }
}
