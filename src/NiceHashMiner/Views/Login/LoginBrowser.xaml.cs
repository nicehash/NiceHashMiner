using Newtonsoft.Json;
using NHM.Common;
using NHMCore;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Web.UI;

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
            Closing += LoginBrowser_Closing;
            Unloaded += LoginBrowser_Unloaded;
            IsVisibleChanged += LoginBrowser_IsVisibleChanged;
        }

        private static string _userAgent => "NiceHashMiner/" + System.Windows.Forms.Application.ProductVersion;

        private void NavigateTo(string url)
        {
            WebViewBrowser.CoreWebView2.Navigate(url);
        }

        private void LoginBrowser_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldIsVisible = e.OldValue as bool?;

            var wasVisible = oldIsVisible.HasValue && oldIsVisible.Value;
            var wasInvisible = oldIsVisible.HasValue && !oldIsVisible.Value;
            if (wasVisible)
            {
                CancelNavigateAndCheck();
                LoadingPanel.Visibility = Visibility.Visible;
                WebViewGrid.Visibility = Visibility.Collapsed;
                NavigateTo("about:blank");
            }
            else if (wasInvisible)
            {
                Logger.Info("LoginBrowser", $"LoginBrowser_IsVisibleChanged e.OldValue={e.OldValue}; e.NewValue={e.NewValue}");
                _ = StartNavigateAndCheck();
            }
            else
            {
                Logger.Error("LoginBrowser", "Unexpected case");
            }
        }

        public bool AllowClose { get; set; } = false;

        public void ForceCleanup()
        {
            WebViewBrowser.Dispose();
            try
            {
                Process.GetProcessById((int)WebViewBrowser.CoreWebView2.BrowserProcessId)?.Kill();
            }
            catch
            { }
        }

        private void LoginBrowser_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelNavigateAndCheck();
        }

        private void LoginBrowser_Closing(object sender, CancelEventArgs e)
        {
            if (!AllowClose)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            CancelNavigateAndCheck();
        }

        private CancellationTokenSource _cts;
        private DateTime _navigationStart = DateTime.MinValue;

        public bool? LoginSuccess { get; private set; } = null;

        private bool? _isOnLoginPage { get; set; } = null;

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            WebViewBrowser.NavigationCompleted += Browser_NavigationCompleted;
        }

        private void CancelNavigateAndCheck()
        {
            try
            {
                _cts?.Cancel();
            }
            catch
            { }
        }

        private async Task StartNavigateAndCheck()
        {
            LabelStatus.Content = Translations.Tr("Loading, please wait...");
            TryAgainButton.Visibility = Visibility.Collapsed;
            ReturnButton.Visibility = Visibility.Collapsed;
            CancelNavigateAndCheck();
            using (_cts = CancellationTokenSource.CreateLinkedTokenSource(ApplicationStateManager.ExitApplication.Token))
            {
                await NavigateAndCheck(_cts.Token);
            }
        }

        private async Task NavigateAndCheck(CancellationToken stop)
        {
            _navigationStart = DateTime.UtcNow;
            await WebViewBrowser.EnsureCoreWebView2Async();
            WebViewBrowser.NavigationCompleted += Browser_NavigationCompleted;
            var urlEncoded = $"{Links.Login}?nhm=1&client={_userAgent}";
            NavigateTo(urlEncoded);
            bool isActive() => !stop.IsCancellationRequested;
            while (isActive())
            {
                await TaskHelpers.TryDelay(TimeSpan.FromSeconds(1), stop);
                var ok = await CheckForBtc();
                if (!ok.HasValue) continue;

                LoginSuccess = ok;
                Hide();
                return;
            }
        }

        private void Browser_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            Logger.InfoDelayed("Login", $"Navigation to {e.NavigationId} {e.WebErrorStatus}", TimeSpan.FromSeconds(5));
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _ = StartNavigateAndCheck();
        }

        const string _jsEvalCode = @"
        (() => {
            try {
                const nhmResponse = document.getElementById('nhmResponse');
                if (!nhmResponse) return 'NO_RESPONSE';
                return nhmResponse.value;
            } catch (error) {
                return 'INJ_ERROR ' + error;
            }
        })();";

        private class Response
        {
            public string btcAddress { get; set; }
        }

        private async Task<bool?> CheckForBtc()
        {
            var showRetry = (DateTime.UtcNow - _navigationStart).TotalSeconds > 5;
            string htmlEvalValue = null;
            try
            {
                htmlEvalValue = await WebViewBrowser.ExecuteScriptAsync(_jsEvalCode);
                var btcAddr = htmlEvalValue.Remove(htmlEvalValue.Length - 1, 1).Remove(0, 1).Replace("\\\"", "\"");
                Logger.InfoDelayed("Login", $"JS eval returned htmlEvalValue='{btcAddr}'", TimeSpan.FromSeconds(15));
                
                var noNhmResponse = htmlEvalValue == null || htmlEvalValue == "NO_RESPONSE" || htmlEvalValue.Contains("INJ_ERROR");
                if (showRetry)
                {
                    LabelStatus.Content = Translations.Tr("Something went wrong");
                    TryAgainButton.Visibility = Visibility.Visible;
                    ReturnButton.Visibility = Visibility.Visible;
                }
                if (noNhmResponse) return null;


                if (LoadingPanel.Visibility != Visibility.Collapsed)
                {
                    LoadingPanel.Visibility = Visibility.Collapsed;
                    WebViewGrid.Visibility = Visibility.Visible;
                }

                var webResponse = JsonConvert.DeserializeObject<Response>(btcAddr);
                if (webResponse == null) return null;
                if (webResponse.btcAddress != null)
                {
                    var result = await ApplicationStateManager.SetBTCIfValidOrDifferent(webResponse.btcAddress);
                    if (result == ApplicationStateManager.SetResult.CHANGED)
                    {
                        Logger.Info("Login", $"Navigation and processing successfull.");
                        return true;
                    }

                    Logger.Error("Login", $"Btc address: {webResponse.btcAddress} was not saved. Result: {result}.");
                }
                else
                {
                    Logger.Info("Login", $"Navigation and processing successfull. BTC wasn't retreived.");
                }
                return false;
            }
            catch (Exception e)
            {
                if (showRetry)
                {
                    LabelStatus.Content = Translations.Tr("Something went wrong");
                    TryAgainButton.Visibility = Visibility.Visible;
                    ReturnButton.Visibility = Visibility.Visible;
                }
                Logger.ErrorDelayed("Login", $"CheckForBtc error: {e}. htmlEvalValue='{htmlEvalValue}'", TimeSpan.FromSeconds(15));
                return null;
            }
        }
    }
}
