using NHM.Common;
using NHMCore;
using NHMCore.Scripts;
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
    /// Interaction logic for NhmScriptSimple.xaml
    /// </summary>
    public partial class NhmScriptSimple : Window
    {
        private ObservableCollection<ScriptDisplay> AddedScripts = new ObservableCollection<ScriptDisplay>();

        public NhmScriptSimple()
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string code = codeBox.Text;
            codeBox.Text = "";
            var scriptId = JSBridge.AddJSScript(code);
            var newScript = new ScriptDisplay($"Script {scriptId}", scriptId, code);
            AddedScripts.Add(newScript);
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
        public class ScriptDisplay : NotifyChangedBase
        {
            public ScriptDisplay(string title, long id, string jsCode)
            {
                Title = title;
                Id = id;
                Code = jsCode;
            }
            public long Id { get; set; }
            public string Code { get; set; }
            public string Title { get; private set; }

            public string Error { get; set; }

            public string ErrorStack { get; set; }
        }
    }

    
}

