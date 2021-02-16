using NiceHashMiner.Views.Common.NHBase;
using System.Windows;

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for SingleInstanceNotice.xaml
    /// </summary>
    public partial class SingleInstanceNotice : BaseDialogWindow
    {
        public SingleInstanceNotice()
        {
            InitializeComponent();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
