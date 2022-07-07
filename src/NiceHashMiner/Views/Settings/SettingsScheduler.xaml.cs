using NHMCore.Schedules;
using NHMCore.Utils;
using NiceHashMiner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsScheduler.xaml
    /// </summary>
    public partial class SettingsScheduler : UserControl
    {
        public SettingsScheduler()
        {
            InitializeComponent();
        }

        private void btn_new_slot_Click(object sender, RoutedEventArgs e)
        {
            var pattern = "[0-2][0-9]:[0-5][0-9]";
            var rg = new Regex(pattern);
            var anyDay = (bool)cboxMon.IsChecked || (bool)cboxTue.IsChecked || (bool)cboxWed.IsChecked
                || (bool)cboxThu.IsChecked || (bool)cboxFri.IsChecked || (bool)cboxSat.IsChecked || (bool)cboxSun.IsChecked;
            var rightFormat = rg.IsMatch(textBoxSchedulerFrom.Text) && rg.IsMatch(textBoxSchedulerTo.Text);

            
            var timeComparation = rightFormat && Convert.ToDateTime(textBoxSchedulerFrom.Text) < Convert.ToDateTime(textBoxSchedulerTo.Text);

            if (anyDay && rightFormat && timeComparation)
            {
                var schedule = new Schedule()
                {
                    From = textBoxSchedulerFrom.Text,
                    To = textBoxSchedulerTo.Text,
                    Days = new Dictionary<string, bool>()
                    {
                        ["Monday"] = (bool)cboxMon.IsChecked,
                        ["Tuesday"] = (bool)cboxTue.IsChecked,
                        ["Wednesday"] = (bool)cboxWed.IsChecked,
                        ["Thursday"] = (bool)cboxThu.IsChecked,
                        ["Friday"] = (bool)cboxFri.IsChecked,
                        ["Saturday"] = (bool)cboxSat.IsChecked,
                        ["Sunday"] = (bool)cboxSun.IsChecked,
                    }
                };
                SchedulesManager.Instance.AddScheduleToList(schedule);
                SetDefaults();
            }
        }

        private void SetDefaults()
        {
            textBoxSchedulerFrom.Text = "hh:mm";
            textBoxSchedulerTo.Text = "hh:mm";
            cboxMon.IsChecked = false;
            cboxTue.IsChecked = false;
            cboxWed.IsChecked = false;
            cboxThu.IsChecked = false;
            cboxFri.IsChecked = false;
            cboxSat.IsChecked = false;
            cboxSun.IsChecked = false;
        }

        private void textBoxSchedulerFrom_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (textBoxSchedulerFrom.Text == "hh:mm") textBoxSchedulerFrom.Text = "";
        }

        private void textBoxSchedulerTo_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (textBoxSchedulerTo.Text == "hh:mm") textBoxSchedulerTo.Text = "";
        }

        private void textBoxSchedulerFrom_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (textBoxSchedulerFrom.Text == "") textBoxSchedulerFrom.Text = "hh:mm";
        }

        private void textBoxSchedulerTo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (textBoxSchedulerTo.Text == "") textBoxSchedulerTo.Text = "hh:mm";
        }

        private void textBoxSchedulerFrom_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var pattern = "[0-9]|:";
            var rg = new Regex(pattern);
            if (!rg.IsMatch(e.Text)) e.Handled = true;
        }

        private void textBoxSchedulerTo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var pattern = "[0-9]|:";
            var rg = new Regex(pattern);
            if (!rg.IsMatch(e.Text)) e.Handled = true;
        }
    }
}
