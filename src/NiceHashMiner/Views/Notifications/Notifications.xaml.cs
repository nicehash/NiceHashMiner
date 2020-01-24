using System.Windows.Controls;

namespace NiceHashMiner.Views.Notifications
{
    /// <summary>
    /// Interaction logic for Notifications.xaml
    /// </summary>
    public partial class Notifications : UserControl
    {
        public Notifications()
        {
            InitializeComponent();
        }

        // TODO show icon for new notification
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach(NHMCore.Notifications.Notification nekaj in ic_NotificationsList.ItemsSource)
            {
                if(nekaj is NHMCore.Notifications.Notification)
                {
                    nekaj.NotificationNew = false;
                }
            }   
        }
    }
}
