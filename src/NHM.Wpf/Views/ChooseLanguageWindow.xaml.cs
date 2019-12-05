using NHM.Wpf.ViewModels;
using NHM.Wpf.Views.Common;
using NHM.Wpf.Views.Common.NHBase;
using System.Windows;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ChooseLanguageWindow.xaml
    /// </summary>
    public partial class ChooseLanguageWindow : BaseDialogWindow
    {
        private readonly ChooseLanguageVM _vm;

        public ChooseLanguageWindow()
        {
            InitializeComponent();
            _vm = this.AssertViewModel<ChooseLanguageVM>();
            WindowUtils.InitWindow(this);
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm?.Dispose();
            WindowUtils.Window_OnClosing(this);
        }
    }
}
