using NHMCore;
using NHMCore.Configs;
using NHMCore.Notifications;
using NiceHashMiner.Views.Common;
using System;
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
        private bool _isSingleShot = false;
        public NotificationItem()
        {
            InitializeComponent();
            DataContextChanged += NotificationItemItem_DataContextChanged;
            WindowUtils.Translate(this);
        }

        private void NotificationItemItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Notification notification)
            {
                _notification = notification;
                var action = _notification.Action;
                if (action != null)
                {
                    ActionButton.Content = action.Info;
                    ActionButton.Visibility = Visibility.Visible;
                    _isSingleShot = action.IsSingleShotAction;
                    if (action.BindProgress)
                    {
                        action.Progress = new Progress<int>((int p) =>
                        {
                            ActionProgressBar.Value = p;
                        });
                    }
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
            if (_isSingleShot) ActionButton.IsEnabled = false;
            var action = _notification.Action;
            if (action != null && action.Action != null) action.Action?.Invoke();
            if (action != null && action.BindProgress) ActionProgress.Visibility = Visibility.Visible;
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
            InfoToggleButtonText.Text = Translations.Tr(InfoToggleButtonText.Text);
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
