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
        public string Placeholder = "hh:mm";
        public SettingsScheduler()
        {
            InitializeComponent();
        }

        private void btn_new_slot_Click(object sender, RoutedEventArgs e)
        {
            var schedulerFrom = ShouldPrepend(textBoxSchedulerFrom.Text) ? "0" + textBoxSchedulerFrom.Text : textBoxSchedulerFrom.Text;
            var schedulerTo = ShouldPrepend(textBoxSchedulerTo.Text) ? "0" + textBoxSchedulerTo.Text : textBoxSchedulerTo.Text;
            var anyDay = (bool)cboxMon.IsChecked || (bool)cboxTue.IsChecked || (bool)cboxWed.IsChecked
                || (bool)cboxThu.IsChecked || (bool)cboxFri.IsChecked || (bool)cboxSat.IsChecked || (bool)cboxSun.IsChecked;
            var rightFormat = ValidateHour(schedulerFrom) && ValidateHour(schedulerTo);
            var isNotSame = schedulerFrom != schedulerTo;

            if (anyDay && rightFormat && isNotSame)
            {
                var isNextDay = Convert.ToDateTime(schedulerTo) < Convert.ToDateTime(schedulerFrom);
                if (isNextDay)
                {
                    var schedule = new Schedule()
                    {
                        From = schedulerFrom,
                        To = schedulerTo,
                        DaysFrom = new Dictionary<string, bool>()
                        {
                            ["Monday"] = (bool)cboxMon.IsChecked,
                            ["Tuesday"] = (bool)cboxTue.IsChecked,
                            ["Wednesday"] = (bool)cboxWed.IsChecked,
                            ["Thursday"] = (bool)cboxThu.IsChecked,
                            ["Friday"] = (bool)cboxFri.IsChecked,
                            ["Saturday"] = (bool)cboxSat.IsChecked,
                            ["Sunday"] = (bool)cboxSun.IsChecked,
                        },
                        DaysTo = new Dictionary<string, bool>()
                        {
                            ["Monday"] = (bool)cboxSun.IsChecked,
                            ["Tuesday"] = (bool)cboxMon.IsChecked,
                            ["Wednesday"] = (bool)cboxTue.IsChecked,
                            ["Thursday"] = (bool)cboxWed.IsChecked,
                            ["Friday"] = (bool)cboxThu.IsChecked,
                            ["Saturday"] = (bool)cboxFri.IsChecked,
                            ["Sunday"] = (bool)cboxSat.IsChecked,
                        }
                    };
                    SchedulesManager.Instance.AddScheduleToList(schedule);
                }
                else
                {
                    var schedule = new Schedule()
                    {
                        From = schedulerFrom,
                        To = schedulerTo,
                        DaysFrom = new Dictionary<string, bool>()
                        {
                            ["Monday"] = (bool)cboxMon.IsChecked,
                            ["Tuesday"] = (bool)cboxTue.IsChecked,
                            ["Wednesday"] = (bool)cboxWed.IsChecked,
                            ["Thursday"] = (bool)cboxThu.IsChecked,
                            ["Friday"] = (bool)cboxFri.IsChecked,
                            ["Saturday"] = (bool)cboxSat.IsChecked,
                            ["Sunday"] = (bool)cboxSun.IsChecked,
                        },
                        DaysTo = new Dictionary<string, bool>()
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
                }
                SetDefaults();
            }
        }

        private void SetDefaults()
        {
            textBoxSchedulerFrom.Text = "hh:mm";
            textBoxSchedulerTo.Text = "hh:mm";
            cboxMon.IsChecked = true;
            cboxTue.IsChecked = true;
            cboxWed.IsChecked = true;
            cboxThu.IsChecked = true;
            cboxFri.IsChecked = true;
            cboxSat.IsChecked = true;
            cboxSun.IsChecked = true;
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
            else
            {
                var hourOK = ValidateHour(textBoxSchedulerFrom.Text);
                var (style, brush) = GetStyleBrush(hourOK);
                textBoxSchedulerFrom.Style = style;
                textBoxSchedulerFrom.BorderBrush = brush;
            }
        }

        private void textBoxSchedulerTo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (textBoxSchedulerTo.Text == "") textBoxSchedulerTo.Text = "hh:mm";
            else
            {
                var hourOK = ValidateHour(textBoxSchedulerTo.Text);
                var (style, brush) = GetStyleBrush(hourOK);
                textBoxSchedulerTo.Style = style;
                textBoxSchedulerTo.BorderBrush = brush;
            }
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
        private (Style style, Brush brush) GetStyleBrush(bool isGood)
        {
            var (styleName, brushName) = isGood ? ("InputBoxGoodSmall", "NastyGreenBrush") : ("InputBoxBadSmall", "RedDangerColorBrush");
            return (
                Application.Current.FindResource(styleName) as Style,
                (Brush)Application.Current.FindResource(brushName)
                );
        }

        private bool ValidateHour(string hour)
        {
            var pattern = "^[0-1][0-9]:[0-5][0-9]$";
            var pattern2 = "^[2][0-3]:[0-5][0-9]$";
            var pattern3 = "^[0-9]:[0-5][0-9]$";
            var rg = new Regex(pattern);
            var rg2 = new Regex(pattern2);
            var rg3 = new Regex(pattern3);

            return rg.IsMatch(hour) || rg2.IsMatch(hour) || rg3.IsMatch(hour);
        }

        private bool ShouldPrepend(string hour)
        {
            var pattern = "[0-9]:[0-5][0-9]";
            var pattern1 = "[0-2][0-9]:[0-5][0-9]";
            var rg = new Regex(pattern);
            var rg1 = new Regex(pattern1);
            if (rg.IsMatch(hour) && !rg1.IsMatch(hour))
                return true;
            return false;
        }

        private void textBoxSchedulerFrom_KeyUp(object sender, KeyEventArgs e)
        {
            var hourOK = ValidateHour(textBoxSchedulerFrom.Text);
            var (style, brush) = GetStyleBrush(hourOK);
            textBoxSchedulerFrom.Style = style;
            textBoxSchedulerFrom.BorderBrush = brush;
        }

        private void textBoxSchedulerTo_KeyUp(object sender, KeyEventArgs e)
        {
            var hourOK = ValidateHour(textBoxSchedulerTo.Text);
            var (style, brush) = GetStyleBrush(hourOK);
            textBoxSchedulerTo.Style = style;
            textBoxSchedulerTo.BorderBrush = brush;
        }

        private void textBoxSchedulerFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBoxSchedulerFrom == null) return;
            if (textBoxSchedulerFrom.Text == Placeholder) return;
            var hourOK = ValidateHour(textBoxSchedulerFrom.Text);
            var (style, brush) = GetStyleBrush(hourOK);
            textBoxSchedulerFrom.Style = style;
            textBoxSchedulerFrom.BorderBrush = brush;
        }

        private void textBoxSchedulerTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBoxSchedulerTo == null) return;
            if (textBoxSchedulerTo.Text == Placeholder) return;
            var hourOK = ValidateHour(textBoxSchedulerTo.Text);
            var (style, brush) = GetStyleBrush(hourOK);
            textBoxSchedulerTo.Style = style;
            textBoxSchedulerTo.BorderBrush = brush;
        }
    }
}
