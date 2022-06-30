using NHMCore.Schedules;
using NHMCore.Utils;
using NiceHashMiner.ViewModels;
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
            SchedulesManager.Instance.AddScheduleToList();
        }
    }
}
