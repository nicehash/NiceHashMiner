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
            //checkItems();
        }

        private void checkItems()
        {
            if (ic_NotificationsList.Items.Count == 0) ic_NotificationsList.Items.Add(new Label() { Content = "No new notifications" });
        }
    }
}
