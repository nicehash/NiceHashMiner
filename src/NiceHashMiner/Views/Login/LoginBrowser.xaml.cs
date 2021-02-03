using Newtonsoft.Json;
using NHM.Common;
using NHMCore;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginBrowser.xaml
    /// </summary>
    public partial class LoginBrowser : Window
    {
        public LoginBrowser()
        {
            InitializeComponent();
        }

        private object _lock = new object();
        private Timer _evalTimer;
        private bool _canRefresh = false;
        internal class TryLock : IDisposable
        {
            private object locked;
            public bool HasAcquiredLock { get; private set; }
            public TryLock(object obj)
            {
                if (Monitor.TryEnter(obj))
                {
                    HasAcquiredLock = true;
                    locked = obj;
                }
            }
            public void Dispose()
            {
                if (HasAcquiredLock)
                {
                    Monitor.Exit(locked);
                    locked = null;
                    HasAcquiredLock = false;
                }
            }
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            browser.NavigationCompleted += Browser_NavigationCompleted;
            NavigateAndStartTimer();
        }

        private void NavigateAndStartTimer()
        {
            browser.Navigate(Links.LoginNHM);
            _evalTimer = new Timer((s) => { Dispatcher.Invoke(EvalTimer_Elapsed); }, null, 100, 1000);
        }

        private void Browser_NavigationCompleted(object sender, WebViewControlNavigationCompletedEventArgs e)
        {
            if(e.IsSuccess == false)
            {
                btn_refresh.Visibility = Visibility.Visible;
                _canRefresh = true;
                Logger.Error("Login", $"Navigation to {e.Uri.ToString()} failed with error: {e.WebErrorStatus.ToString()}");
            }
            else
            {
                btn_refresh.Visibility = Visibility.Collapsed;
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (_canRefresh)
            {
                _canRefresh = false;
                NavigateAndStartTimer();                
            }           
        }

        private async void EvalTimer_Elapsed()
        {
            await EvalTimer_ElapsedTask();
        }

        private async Task EvalTimer_ElapsedTask()
        {
            using (var tryLock = new TryLock(_lock))
            {
                if (!tryLock.HasAcquiredLock) return;
                try
                {
                    string html = await browser.InvokeScriptAsync("eval", new string[] { "document.getElementById('nhmResponse').value;" });
                    if (!html.Contains("btcAddress")) return;
                    var btcResp = JsonConvert.DeserializeObject<BtcResponse>(html);
                    if(btcResp.btcAddress != null)
                    {
                        var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(btcResp.btcAddress);

                        var btcLoginDialog = new CustomDialog()
                        {
                            Title = Translations.Tr("Login"),
                            OkText = Translations.Tr("Ok"),
                            CancelVisible = Visibility.Collapsed,
                            AnimationVisible = Visibility.Collapsed
                        };

                        if (result == ApplicationStateManager.SetResult.INVALID)
                        {
                            btcLoginDialog.Description = Translations.Tr("Unable to retreive BTC address. Please retreive it by yourself from web page.");
                            btcLoginDialog.OKClick += (s, e) => {
                                Process.Start(Links.Login);
                            };
                        }
                        else if(result == ApplicationStateManager.SetResult.CHANGED)
                        {
                            btcLoginDialog.Description = Translations.Tr("Login performed successfully. Feel free to start mining.");
                        }

                        CustomDialogManager.ShowModalDialog(btcLoginDialog);
                        Logger.Info("Login", $"Navigation and processing successfull.");
                    }
                    else
                    {
                        var btcLoginDialog = new CustomDialog()
                        {
                            Title = Translations.Tr("Login"),
                            OkText = Translations.Tr("Ok"),
                            CancelVisible = Visibility.Collapsed,
                            AnimationVisible = Visibility.Collapsed,
                            Description = Translations.Tr("Unable to retreive BTC address. Please retreive it by yourself from web page.")
                        };
                        btcLoginDialog.OKClick += (s, e) => {
                            Process.Start(Links.Login);
                        };
                        CustomDialogManager.ShowModalDialog(btcLoginDialog);
                        Logger.Info("Login", $"Navigation and processing successfull. BTC wasn't retreived.");
                    }
                    _evalTimer.Dispose();
                    Close();
                }
                catch (Exception e)
                {
                    Logger.Error("Login", e.Message);
                }
            }
        }

        [Serializable]
        private class BtcResponse
        {
            public string btcAddress { get; set; }
        }      
    }
}
