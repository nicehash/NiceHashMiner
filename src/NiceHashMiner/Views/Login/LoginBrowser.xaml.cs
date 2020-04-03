using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using Newtonsoft.Json;
using NHMCore;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using System;
using System.Diagnostics;
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

        private object _lock = new object();
        private Timer _evalTimer;
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
            browser.Navigate("https://test-dev.nicehash.com/my/login?nhm=1");
            _evalTimer = new Timer((s) => { Dispatcher.Invoke(EvalTimer_Elapsed); }, null, 100, 1000);

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
                        _evalTimer.Dispose();
                        Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void BtcLoginDialog_OKClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        [Serializable]
        private class BtcResponse
        {
            public string btcAddress { get; set; }
        }
    }
}
