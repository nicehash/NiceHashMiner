using NHMCore.Configs;
using NHMCore.Notifications;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationItem.xaml
    /// </summary>
    public partial class NotificationItem : UserControl
    {

        private Notification _notification;
        public NotificationItem()
        {
            InitializeComponent();
            DataContextChanged += NotificationItemItem_DataContextChanged;
        }

        private void NotificationItemItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Notification notification)
            {
                _notification  = notification;
                var baseAction = _notification.Actions.FirstOrDefault();
                if (baseAction is NotificationAction action)
                {
                    ActionButton.Content = action.Info;
                    ActionButton.Click += (s, be) => action.Action?.Invoke();
                    ActionButton.Visibility = Visibility.Visible;
                }
                return;
            }
            throw new Exception("unsupported datacontext type");
        }

        private void RemoveNotification(object sender, RoutedEventArgs e)
        {
            _notification.RemoveNotification();
            if (!string.IsNullOrEmpty(_notification.NotificationUUID))
            {
                MiscSettings.Instance.ShowNotifications.Remove(_notification.NotificationUUID);
                MiscSettings.Instance.ShowNotifications.Add(_notification.NotificationUUID, DontShowAgainCheckBox?.IsChecked ?? false);
                ConfigManager.GeneralConfigFileCommit();
            }
        }

        private void ExecuteNotificationAction(object sender, RoutedEventArgs e)
        {

        }

        private void InfoToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (InfoToggleButton.IsChecked.Value)
            {
                Expand();
            }
            else
            {
                Collapse();
            }
        }

        private void Collapse()
        {
            notificationsDetailsGrid.Visibility = Visibility.Collapsed;
            InfoToggleButton.IsChecked = false;
            InfoToggleButtonText.Text = "More Info";
        }

        private void Expand()
        {
            notificationsDetailsGrid.Visibility = Visibility.Visible;
            InfoToggleButton.IsChecked = true;
            InfoToggleButtonText.Text = "Less Info";
        }
    }
}
