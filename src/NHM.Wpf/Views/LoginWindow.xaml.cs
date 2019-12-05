using NHM.Wpf.Views.Common;
using NHM.Wpf.Views.Common.NHBase;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            WindowUtils.Translate(this);
        }

        private void CheckBoxMode_Checked(object sender, RoutedEventArgs e)
        {
            SwitchTheme(false);
            var drawingBrushFromResources = this.FindResource("drawingBrush_NightCircleNH") as DrawingBrush;
            drawingBrushNH.Drawing = drawingBrushFromResources.Drawing;
        }

        private void CheckBoxMode_Unchecked(object sender, RoutedEventArgs e)
        {
            SwitchTheme(true);
            var drawingBrushFromResources = this.FindResource("drawingBrush_circleNH") as DrawingBrush;
            drawingBrushNH.Drawing = drawingBrushFromResources.Drawing;
        }

        private void Register_OnClick(object sender, RoutedEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            Process.Start(hyperlink.NavigateUri.ToString());
        }

        private void ManuallyEnterBtc_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
