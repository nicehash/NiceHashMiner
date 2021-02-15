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
using System.Collections.Generic;
using System.Net.Http;

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
            var headers = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("User-Agent", "NHM/" + System.Windows.Forms.Application.ProductVersion) };
            browser.Navigate(new Uri(Links.LoginNHM), HttpMethod.Get, null, headers);
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
                    #warning handle case for logged in sessions with/without btc
                    string html = await browser.InvokeScriptAsync("eval", new string[] { "document.getElementById('nhmResponse').value;" });
                    var webResponse = JsonConvert.DeserializeObject<Response>(html);
                    if (webResponse == null) return;

                    // assume failure
                    var description = Translations.Tr("Unable to retreive BTC address. Please retreive it by yourself from web page.");

                    if (webResponse.btcAddress != null)
                    {
                        var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(webResponse.btcAddress);
                        if (result == ApplicationStateManager.SetResult.CHANGED)
                        {
                            description = Translations.Tr("Login performed successfully.");
                            Logger.Info("Login", $"Navigation and processing successfull.");
                        }
                        else 
                        {
                            Logger.Error("Login", $"Btc address: {webResponse.btcAddress} was not saved. Result: {result}.");
                        }

                    }
                    else if (webResponse.error != null)
                    {
                        var error = webResponse.error;
                        Logger.Error("Login", "Received error: " + error);
                    }
                    else
                    {
                        Logger.Info("Login", $"Navigation and processing successfull. BTC wasn't retreived.");
                    }

                    var btcLoginDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("Login"),
                        OkText = Translations.Tr("Ok"),
                        CancelVisible = Visibility.Collapsed,
                        AnimationVisible = Visibility.Collapsed,
                        Description = description
                    };
                    btcLoginDialog.OKClick += (s, e) => {
                        Process.Start(Links.Login);
                    };

                    CustomDialogManager.ShowModalDialog(btcLoginDialog);

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
        private class Response
        {
            public string btcAddress { get; set; }
            public string error { get; set; }
        }      
    }
}
