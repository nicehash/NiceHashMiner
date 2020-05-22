using NHM.Common;
using NHMCore;
using NHMCore.Scripts;
using NiceHashMiner.Views.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace NiceHashMiner
{
    /// <summary>
    /// Interaction logic for NhmScript.xaml
    /// </summary>
    public partial class NhmScript : Window
    {
        private ObservableCollection<ScriptDisplay> AddedScripts = new ObservableCollection<ScriptDisplay>();

        public NhmScript()
        {
            InitializeComponent();
            this.DataContext = AddedScripts;
            JSBridge.OnJSErrorCallback = (string error, string stack, Int64 script_id) => {
                this.Dispatcher.Invoke(() =>
                {
                    var unloadedScript = this.AddedScripts.Where(script => script.Id == script_id).FirstOrDefault();
                    if (unloadedScript == null) return;
                    unloadedScript.ErrorStack = stack;
                    unloadedScript.Error = error;
                    unloadedScript.OnPropertyChanged("Error");
                    unloadedScript.OnPropertyChanged("ErrorStack");
                });
            };
            base.Loaded += new RoutedEventHandler(this.OnLoadedSetRender);
        }

        private void OnLoadedSetRender(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }

        private void AddScript(bool isSwitching)
        {
            string code = codeBox.Text;
            codeBox.Text = "";
            var scriptId = JSBridge.AddJSScript(code, isSwitching);
            var newScript = new ScriptDisplay($"Script {scriptId}", scriptId, code, isSwitching);
            AddedScripts.Add(newScript);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddScript(false);
        }

        private void Button_Switching_Click(object sender, RoutedEventArgs e)
        {
            AddScript(true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //JSBridge.StartLoops(ApplicationStateManager.ExitApplication.Token);
        }

        private void Delete_click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ScriptDisplay sd)
            {
                Dispatcher.Invoke(() => JSBridge.RemoveJSScrip(sd.Id));
                AddedScripts.Remove(sd);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tab)
            {
                if (tab.SelectedIndex < 0)
                {
                    codeBoxReadOnly.Text = "";
                }
                else if (tab.Items[tab.SelectedIndex] is ScriptDisplay sd) {
                    codeBoxReadOnly.Text = sd.Code;
                } 
            }
        }
        public class ScriptDisplay : NotifyChangedBase
        {
            public ScriptDisplay(string title, long id, string jsCode, bool isSwitchScript)
            {
                Title = title;
                Id = id;
                Code = jsCode;
                IsSwitching = isSwitchScript ? "SWITCHING" : "";
            }
            public long Id { get; set; }
            public string Code { get; set; } = "";
            public string Title { get; private set; } = "";
            public string IsSwitching { get; private set; } = "";

            public string Error { get; set; } = "";

            public string ErrorStack { get; set; } = "";
        }
    }
}
