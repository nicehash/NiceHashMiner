using NiceHashMiner.ViewModels;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using System.Windows;

namespace NiceHashMiner.Views
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
            Closed += ChooseLanguageWindow_Closed;
        }

        private void ChooseLanguageWindow_Closed(object sender, System.EventArgs e)
        {
            _vm?.Dispose();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
