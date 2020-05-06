using NHMCore;
using NHMCore.Scripts;
using System;
using System.Collections.Generic;
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
        int ScriptCounter = 0;
        public NhmScript()
        {
            InitializeComponent();
        }
        //// WORKING on GUI thread
        //private static bool _initialized = false;
        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    string code = codeBox.Text;
        //    codeBox.Text = "";
        //    if (!_initialized)
        //    {
        //        _initialized = true;
        //        //JSBridge.StartLoops(ApplicationStateManager.ExitApplication.Token);
        //        JSBridge.RegisterNHN_CSharp_JS_Bridge();
        //        //JSBridge.AddScriptAndTick(code);
        //        return;
        //    }

        //    //JSBridge.EvaluateJSExec(code);
        //    Dispatcher.Invoke(() => JSBridge.EvaluateJS(code));
        //    //Dispatcher.Invoke(() => JSBridge.EvaluateJS(code));
        //    //Dispatcher.Invoke(() => JSBridge.ExecTestCall());
        //}

        // NOT WORKING
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string code = codeBox.Text;
            codeBox.Text = "";
            Dispatcher.Invoke(() => JSBridge.EvaluateJS(code));
            lbx_sripts.Items.Add(new ScriptDisplay(ScriptCounter, code.Substring(0,10)));
            ScriptCounter++;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //JSBridge.StartLoops(ApplicationStateManager.ExitApplication.Token);
        }

        private void Delete_click(object sender, RoutedEventArgs e)
        {
            var selectedScript = lbx_sripts.SelectedItem as ScriptDisplay;
            if (selectedScript == null) return;
            Dispatcher.Invoke(() => JSBridge.RemoveScript(selectedScript.Id));
            lbx_sripts.Items.Remove(selectedScript);
        }
    }

    public class ScriptDisplay
    {
        public ScriptDisplay(int id, string shortDescription)
        {
            Id = id;
            ShortDescription = shortDescription;
        }
        public int Id { get; set; }
        public string ShortDescription { get; set; }

        public override string ToString()
        {
            return Id.ToString() + " =>  " + ShortDescription;
        }
    }
}
