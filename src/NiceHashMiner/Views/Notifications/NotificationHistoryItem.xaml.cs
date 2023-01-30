using NHMCore.Notifications;
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

namespace NiceHashMiner.Views.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationHistoryItem.xaml
    /// </summary>
    public partial class NotificationHistoryItem : UserControl
    {
        public NotificationHistoryItem()
        {
            InitializeComponent();
        }

        private void RemoveSingleNotification(object sender, RoutedEventArgs e)
        {
            if(DataContext is Notification notification)
            {
                NotificationsManager.Instance.RemoveNotificationFromList(notification);
            }
        }
    }
}
