using NHM.Wpf.Views.Common;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            WindowUtils.Translate(this);
        }

        private void CheckBoxMode_Checked(object sender, RoutedEventArgs e)
        {

            var drawingBrushFromResources = this.FindResource("drawingBrush_NightCircleNH") as DrawingBrush;
            drawingBrushNH.Drawing = drawingBrushFromResources.Drawing;
            this.Background = new SolidColorBrush(Color.FromRgb(1, 13, 21));
        }

        private void CheckBoxMode_Unchecked(object sender, RoutedEventArgs e)
        {
            var drawingBrushFromResources = this.FindResource("drawingBrush_circleNH") as DrawingBrush;
            drawingBrushNH.Drawing = drawingBrushFromResources.Drawing;
            this.Background = new SolidColorBrush(Color.FromRgb(255,255,255));
        }
    }
}
