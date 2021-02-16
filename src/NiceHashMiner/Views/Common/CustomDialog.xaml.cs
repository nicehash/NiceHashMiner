using System;
using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.Common
{
    /// <summary>
    /// Interaction logic for CustomDialog.xaml
    /// </summary>
    public partial class CustomDialog : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(CustomDialog));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(CustomDialog));

        public static readonly DependencyProperty CancelTextProperty =
            DependencyProperty.Register(nameof(CancelText), typeof(string), typeof(CustomDialog));

        public static readonly DependencyProperty OkTextProperty =
            DependencyProperty.Register(nameof(OkText), typeof(string), typeof(CustomDialog));

        public static readonly DependencyProperty CancelVisibleProperty =
            DependencyProperty.Register(nameof(CancelVisible), typeof(Visibility), typeof(CustomDialog));

        public static readonly DependencyProperty OkVisibleProperty =
            DependencyProperty.Register(nameof(OkVisible), typeof(Visibility), typeof(CustomDialog));

        public static readonly DependencyProperty AnimationVisibleProperty =
            DependencyProperty.Register(nameof(AnimationVisible), typeof(Visibility), typeof(CustomDialog));

        public static readonly DependencyProperty ExitVisibleProperty =
            DependencyProperty.Register(nameof(ExitVisible), typeof(Visibility), typeof(CustomDialog));

        public CustomDialog()
        {
            InitializeComponent();
        }

        public CustomDialog(double maxWidth, double maxHeight)
        {
            InitializeComponent();
            mainGrid.MaxWidth = maxWidth;
            mainGrid.MaxHeight = maxHeight;
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            CustomDialogManager.HideCurrentModal();
            OnExit?.Invoke(this, e);
        }

        public event EventHandler<RoutedEventArgs> OnExit;

        public event EventHandler<RoutedEventArgs> OKClick;

        public event EventHandler<RoutedEventArgs> CancelClick;


        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public string CancelText
        {
            get => (string)GetValue(CancelTextProperty);
            set => SetValue(CancelTextProperty, value);
        }

        public string OkText
        {
            get => (string)GetValue(OkTextProperty);
            set => SetValue(OkTextProperty, value);
        }

        public Visibility CancelVisible
        {
            get => (Visibility)GetValue(CancelVisibleProperty);
            set => SetValue(CancelVisibleProperty, value);
        }

        public Visibility OkVisible
        {
            get => (Visibility)GetValue(OkVisibleProperty);
            set => SetValue(OkVisibleProperty, value);
        }

        public Visibility AnimationVisible
        {
            get => (Visibility)GetValue(AnimationVisibleProperty);
            set => SetValue(AnimationVisibleProperty, value);
        }

        public Visibility ExitVisible
        {
            get => (Visibility)GetValue(ExitVisibleProperty);
            set => SetValue(ExitVisibleProperty, value);
        }

        public bool CloseOnOk { get; set; } = true;
        public bool CloseOnCancel { get; set; } = true;

        private Visibility GetVisibility(string text)
        {
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text)) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            if (CloseOnCancel) CloseDialog(sender, e);
            CancelClick?.Invoke(this, e);
        }

        private void ConfirmButtonClicked(object sender, RoutedEventArgs e)
        {
            if (CloseOnOk) CloseDialog(sender, e);
            OKClick?.Invoke(this, e);
        }
    }
}
