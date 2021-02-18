using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using Newtonsoft.Json;
using NHM.Common;
using NHMCore;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

        private CancellationTokenSource _cts;
        private bool _canRefresh = false;

        public bool? LoginSuccess { get; private set; } = null;

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            browser.NavigationCompleted += Browser_NavigationCompleted;
            _ = StartNavigateAndCheck();
        }

        private async Task StartNavigateAndCheck()
        {
            try
            {
                _cts?.Cancel();
            }
            catch
            { }

            using (_cts = CancellationTokenSource.CreateLinkedTokenSource(ApplicationStateManager.ExitApplication.Token))
            {
                await NavigateAndCheck(_cts.Token);
            }
        }

        private async Task NavigateAndCheck(CancellationToken stop)
        {
            var headers = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("User-Agent", "NHM/" + System.Windows.Forms.Application.ProductVersion) };
            browser.NavigationCompleted += Browser_NavigationCompleted;
            browser.Navigate(new Uri(Links.LoginNHM), HttpMethod.Get, null, headers);
            Func<bool> isActive = () => !stop.IsCancellationRequested;
            while (isActive())
            {
                await TaskHelpers.TryDelay(TimeSpan.FromSeconds(1), stop);
                var ok = await CheckForBtc();
                if (!ok.HasValue) continue;

                LoginSuccess = ok;
                Close();
                return;
            }
        }

        private void Browser_NavigationCompleted(object sender, WebViewControlNavigationCompletedEventArgs e)
        {
            if (e.IsSuccess && e.Uri.ToString() == Links.LoginNHM)
            {
                btn_refresh.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_refresh.Visibility = Visibility.Visible;
                _canRefresh = true;
                Logger.Error("Login", $"Navigation to {e.Uri} failed with error: {e.WebErrorStatus}");
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
                _ = StartNavigateAndCheck();
            }
        }

        private async Task<bool?> CheckForBtc()
        {
            try
            {
#warning handle case for logged in sessions with/without btc
                string html = await browser.InvokeScriptAsync("eval", new string[] { "document.getElementById('nhmResponse').value;" });
                var webResponse = JsonConvert.DeserializeObject<Response>(html);
                if (webResponse == null) return null;

                if (webResponse.btcAddress != null)
                {
                    var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(webResponse.btcAddress);
                    if (result == ApplicationStateManager.SetResult.CHANGED)
                    {
                        Logger.Info("Login", $"Navigation and processing successfull.");
                        return true;
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
                return false;
            }
            catch (Exception e)
            {
                Logger.ErrorDelayed("Login", e.Message, TimeSpan.FromSeconds(15));
                return null;
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
