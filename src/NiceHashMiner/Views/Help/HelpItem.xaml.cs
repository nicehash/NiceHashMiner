using NHMCore.Notifications;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.Help
{
    /// <summary>
    /// Interaction logic for HelpItem.xaml
    /// </summary>
    public partial class HelpItem : UserControl
    {

        private Notification _notification;
        public HelpItem()
        {
            InitializeComponent();
            DataContextChanged += HelpItem_DataContextChanged;
        }

        private void HelpItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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
        }

        private void ExecuteNotificationAction(object sender, RoutedEventArgs e)
        {

        }
    }
}
