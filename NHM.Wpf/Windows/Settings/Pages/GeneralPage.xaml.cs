using System.Windows.Controls;
using NHM.Wpf.Windows.Common;

namespace NHM.Wpf.Windows.Settings.Pages
{
    /// <summary>
    /// Interaction logic for GeneralPage.xaml
    /// </summary>
    public partial class GeneralPage : UserControl, ISettingsPage
    {
        public GeneralPage()
        {
            InitializeComponent();

            WindowUtils.Translate(this);
        }
    }
}
