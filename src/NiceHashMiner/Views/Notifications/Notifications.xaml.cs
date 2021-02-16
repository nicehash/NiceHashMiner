using NHM.Common;
using System;
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
            try
            {
                foreach (var item in ic_NotificationsList.ItemsSource)
                {
                    if (item is NHMCore.Notifications.Notification notification)
                    {
                        notification.NotificationNew = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Notifications", ex.Message);
            }
        }
    }
}
