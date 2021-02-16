using NHMCore;
using NHMCore.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NiceHashMiner.Views.Common
{
    /// <summary>
    /// Interaction logic for EnterWorkernameDialog.xaml
    /// </summary>
    public partial class EnterWorkernameDialog : UserControl
    {
        public EnterWorkernameDialog()
        {
            InitializeComponent();
        }

        private void WorkernameValidation()
        {
            var trimmedBtcText = textBoxWorkername.Text.Trim();
            var isOK = CredentialValidators.ValidateWorkerName(trimmedBtcText);
            SaveButton.IsEnabled = isOK;
            if (isOK)
            {
                textBoxWorkername.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxWorkername.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
            }
            else
            {
                textBoxWorkername.Style = Application.Current.FindResource("InputBoxBad") as Style;
                textBoxWorkername.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
        }

        private void TextBoxWorkername_TextChanged(object sender, TextChangedEventArgs e)
        {
            WorkernameValidation();
        }

        private void TextBoxWorkername_KeyUp(object sender, KeyEventArgs e)
        {
            WorkernameValidation();
        }

        private void TextBoxWorkername_LostFocus(object sender, RoutedEventArgs e)
        {
            WorkernameValidation();
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            CustomDialogManager.HideCurrentModal();
        }


        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            var trimmedWorkernameText = textBoxWorkername.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkernameText);
            if (ApplicationStateManager.SetResult.INVALID == result)
            {
                textBoxWorkername.Style = Application.Current.FindResource("InputBoxBad") as Style;
                textBoxWorkername.BorderBrush = (Brush)Application.Current.FindResource("RedDangerColorBrush");
            }
            else
            {
                CustomDialogManager.HideCurrentModal();
                textBoxWorkername.Style = Application.Current.FindResource("InputBoxGood") as Style;
                textBoxWorkername.BorderBrush = (Brush)Application.Current.FindResource("NastyGreenBrush");
                OnWorkernameChangeHack?.Invoke(this, trimmedWorkernameText);
            }
        }

        public EventHandler<string> OnWorkernameChangeHack;
    }
}
