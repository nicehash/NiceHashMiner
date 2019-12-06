using System.Windows.Controls;

namespace NHM.Wpf.Views.Help
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : UserControl
    {
        public Help()
        {
            InitializeComponent();
            checkItems();
        }

        private void checkItems()
        {
            if (ic_NotificationsList.Items.Count == 0) ic_NotificationsList.Items.Add(new Label() { Content = "No new notifications" });
        }
    }
}
