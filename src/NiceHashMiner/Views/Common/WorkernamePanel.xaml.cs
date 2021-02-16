using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.Common
{
    /// <summary>
    /// Interaction logic for MiningLocation.xaml
    /// </summary>
    public partial class WorkernamePanel : UserControl
    {
        private static readonly EnterWorkernameDialog _enterWorkernameDialog = new EnterWorkernameDialog();
        public WorkernamePanel()
        {
            InitializeComponent();
            DataContextChanged += MiningLocation_DataContextChanged;
            _enterWorkernameDialog.OnWorkernameChangeHack += OnWorkernameChangeHack_DataContextChanged;
        }

        private void OnWorkernameChangeHack_DataContextChanged(object sender, string workenrame)
        {
            // TODO editing content will clear binding
            WorkernameButton.Content = workenrame;
            //var bindingExpression = BindingOperations.GetBindingExpression(WorkernameButton, Button.ContentProperty);
            //if (bindingExpression != null) bindingExpression.UpdateTarget();
        }

        private void MiningLocation_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _enterWorkernameDialog.DataContext = e.NewValue;
        }

        private void Click_ChangeWorkername(object sender, RoutedEventArgs e)
        {
            CustomDialogManager.ShowDialog(_enterWorkernameDialog);
        }
    }
}
