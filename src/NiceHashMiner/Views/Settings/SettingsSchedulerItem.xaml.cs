using NHMCore.Schedules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for SettingsSchedulerItem.xaml
    /// </summary>
    public partial class SettingsSchedulerItem : UserControl
    {
        private Schedule _schedule;
        public SettingsSchedulerItem()
        {
            InitializeComponent();
            DataContextChanged += SettingsSchedulerItem_DataContextChanged;
        }

        private void SettingsSchedulerItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is Schedule sc)
            {
                _schedule = sc;
                return;
            }
            throw new Exception("Schedule DataContext be of type" + DataContext.GetType());
        }

        private void DeleteSchedule_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ToggleButton tb) tb.Background = Application.Current.FindResource("TrashLightLogo") as Brush;
        }

        private void DeleteSchedule_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ToggleButton tb) tb.Background = Application.Current.FindResource("TrashDarkLogo") as Brush;
        }

        private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            SchedulesManager.Instance.DeleteScheduleFromList(_schedule);
        }
    }
}
