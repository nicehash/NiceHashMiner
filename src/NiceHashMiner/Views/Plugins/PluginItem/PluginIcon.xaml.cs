using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NiceHashMiner.Views.Plugins.PluginItem
{
    /// <summary>
    /// Interaction logic for PluginIcon.xaml
    /// </summary>
    public partial class PluginIcon : UserControl, IThemeSetter
    {
        public PluginIcon()
        {
            InitializeComponent();
            ThemeSetterManager.AddThemeSetter(this);
        }

        void IThemeSetter.SetTheme(bool isLight)
        {
            if (isLight)
            {
                // LIGHT
                iconBrush.Drawing = Application.Current.FindResource("MiningAxeIcon") as Drawing;
                border.Background = new SolidColorBrush((Color)Application.Current.Resources["Colors.Light.Grey.Grey3"]);
            }
            else
            {
                // DARK
                iconBrush.Drawing = Application.Current.FindResource("MiningAxeIconDark") as Drawing;
                border.Background = Application.Current.Resources["BackgroundColor"] as Brush;
            }
        }
    }
}
