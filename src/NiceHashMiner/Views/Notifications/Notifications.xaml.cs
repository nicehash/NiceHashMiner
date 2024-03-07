using NHM.Common;
using NHMCore.Notifications;
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
            EventManager.Instance.EventAdded += AddToEventBox;
            EventManager.Instance.EventsLoaded += OnLoadedEvents;
        }

        // TODO show icon for new notification
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                foreach (var item in ic_NotificationsList.ItemsSource)
                {
                    if (item is Notification notification)
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
        private void ClearAllNotifications(object sender, System.Windows.RoutedEventArgs e)
        {
            NotificationsManager.Instance.ClearAllNotifications();
        }

        private void PastEvents_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (pastEventsGrid.Visibility == System.Windows.Visibility.Visible)
            {
                pastEventsGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                pastEventsGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private void OnLoadedEvents(object sender, EventArgs e)
        {
            var eventArr = EventManager.Instance.Events;
            foreach (var ev in eventArr)
            {
                WriteToEventBox($"{String.Format("{0:G}", ev.DateTime)} - {ev.Content}");
            }
        }
        private void AddToEventBox(object sender, string newEvent)
        {
           WriteToEventBox(newEvent);
        }
        private void WriteToEventBox(string content)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                pastEventsTB.Text += $"{content}\n";
            }));
        }
    }
}
