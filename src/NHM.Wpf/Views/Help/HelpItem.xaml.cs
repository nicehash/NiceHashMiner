using NHM.Wpf.ViewModels.Help;
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

namespace NHM.Wpf.Views.Help
{
    /// <summary>
    /// Interaction logic for HelpItem.xaml
    /// </summary>
    public partial class HelpItem : UserControl
    {

        private NotificationsElementVM _notification;
        public HelpItem()
        {
            InitializeComponent();
            DataContextChanged += HelpItem_DataContextChanged;
        }

        private void HelpItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is NotificationsElementVM notification)
            {
                _notification  = notification;
                return;
            }
            throw new Exception("unsupported datacontext type");
        }
    }
}
