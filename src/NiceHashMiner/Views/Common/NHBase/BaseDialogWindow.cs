using Microsoft.Win32;
using NiceHashMiner.Views.Common.NHBase.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace NiceHashMiner.Views.Common.NHBase
{
    public partial class BaseDialogWindow : Window, IThemeSetter
    {
        private HwndSource _hwndSource;

        private bool isMouseButtonDown;
        private bool isManualDrag;
        private System.Windows.Point mouseDownPosition;
        private System.Windows.Point positionBeforeDrag;
        private System.Windows.Point previousScreenBounds;

        public Grid WindowRoot { get; private set; }
        public Grid LayoutRoot { get; private set; }
        public System.Windows.Controls.Button MinimizeButton { get; private set; }
        public System.Windows.Controls.Button MaximizeButton { get; private set; }
        public System.Windows.Controls.Button RestoreButton { get; private set; }
        public System.Windows.Controls.Button CloseButton { get; private set; }
        public Grid HeaderBar { get; private set; }
        public Grid HeaderBar2 { get; private set; }
        public double HeightBeforeMaximize { get; private set; }
        public double WidthBeforeMaximize { get; private set; }
        public WindowState PreviousState { get; private set; }

        public Grid NHMIcon { get; private set; }
        public TextBlock WindowTitle { get; private set; }

        static BaseDialogWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseDialogWindow),
                new FrameworkPropertyMetadata(typeof(BaseDialogWindow)));
        }

        public BaseDialogWindow()
        {
            double currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
            Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            base.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
            base.StateChanged += new EventHandler(this.OnStateChanged);
            base.Loaded += new RoutedEventHandler(this.OnLoaded);
            Rectangle workingArea = screen.WorkingArea;
            base.MaxHeight = (double)(workingArea.Height + 16) / currentDPIScaleFactor;
            SystemEvents.DisplaySettingsChanged += new EventHandler(this.SystemEvents_DisplaySettingsChanged);
            this.AddHandler(Window.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.OnMouseButtonUp), true);
            this.AddHandler(Window.MouseMoveEvent, new System.Windows.Input.MouseEventHandler(this.OnMouseMove));
            // extra loaded/closing stuff
            base.Loaded += new RoutedEventHandler(this.OnLoadedSetRender);
            base.Closing += new CancelEventHandler(this.OnWindowClosing);
            ThemeSetterManager.AddThemeSetter(this);
        }

        private void OnLoadedSetRender(object sender, RoutedEventArgs e)
        {
            WindowUtils.InitWindow(this);
            WindowUtils.SetForceSoftwareRendering(this);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            WindowUtils.Window_OnClosing(this);
        }

        public T GetRequiredTemplateChild<T>(string childName) where T : DependencyObject
        {
            return (T)base.GetTemplateChild(childName);
        }

        public override void OnApplyTemplate()
        {
            this.WindowRoot = this.GetRequiredTemplateChild<Grid>("WindowRoot");
            this.LayoutRoot = this.GetRequiredTemplateChild<Grid>("LayoutRoot");
            this.MinimizeButton = this.GetRequiredTemplateChild<System.Windows.Controls.Button>("MinimizeButton");
            this.MaximizeButton = this.GetRequiredTemplateChild<System.Windows.Controls.Button>("MaximizeButton");
            this.RestoreButton = this.GetRequiredTemplateChild<System.Windows.Controls.Button>("RestoreButton");
            this.CloseButton = this.GetRequiredTemplateChild<System.Windows.Controls.Button>("CloseButton");
            this.HeaderBar = this.GetRequiredTemplateChild<Grid>("PART_HeaderBar");
            NHMIcon = this.GetRequiredTemplateChild<Grid>("NHMIcon");
            WindowTitle = this.GetRequiredTemplateChild<TextBlock>("WindowTitle");
            this.HeaderBar2 = this.GetRequiredTemplateChild<Grid>("PART_HeaderBar2");


            if (HideIconAndTitle)
            {
                HideIconAndTitleMethod();
            }

            if (this.LayoutRoot != null && this.WindowState == WindowState.Maximized)
            {
                this.LayoutRoot.Margin = GetDefaultMarginForDpi();
            }

            if (this.CloseButton != null)
            {
                this.CloseButton.Click += CloseButton_Click;
            }

            if (this.MinimizeButton != null)
            {
                this.MinimizeButton.Click += MinimizeButton_Click;
            }

            if (this.RestoreButton != null)
            {
                this.RestoreButton.Click += RestoreButton_Click;
            }

            if (this.MaximizeButton != null)
            {
                this.MaximizeButton.Click += MaximizeButton_Click;
            }

            if (this.HeaderBar != null)
            {
                this.HeaderBar.AddHandler(Grid.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnHeaderBarMouseLeftButtonDown));
            }

            if (this.HeaderBar2 != null)
            {
                this.HeaderBar2.AddHandler(Grid.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnHeaderBarMouseLeftButtonDown));
            }

            base.OnApplyTemplate();
        }

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += OnSourceInitialized;
            base.OnInitialized(e);
        }

        protected virtual void OnHeaderBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isManualDrag)
            {
                return;
            }

            System.Windows.Point position = e.GetPosition(this);
            int headerBarHeight = 36;
            int leftmostClickableOffset = 50;

            if (position.X - this.LayoutRoot.Margin.Left <= leftmostClickableOffset && position.Y <= headerBarHeight)
            {
                if (e.ClickCount != 2)
                {
                    this.OpenSystemContextMenu(e);
                }
                else
                {
                    base.Close();
                }
                e.Handled = true;
                return;
            }

            if (e.ClickCount == 2 && base.ResizeMode == ResizeMode.CanResize)
            {
                this.ToggleWindowState();
                return;
            }

            if (base.WindowState == WindowState.Maximized)
            {
                this.isMouseButtonDown = true;
                this.mouseDownPosition = position;
            }
            else
            {
                try
                {
                    this.positionBeforeDrag = new System.Windows.Point(base.Left, base.Top);
                    this.DragMove();
                }
                catch
                {
                }
            }
        }

        protected void ToggleWindowState()
        {
            if (base.WindowState != WindowState.Maximized)
            {
                base.WindowState = WindowState.Maximized;
            }
            else
            {
                base.WindowState = WindowState.Normal;
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleWindowState();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleWindowState();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        private void SetMaximizeButtonsVisibility(Visibility maximizeButtonVisibility, Visibility reverseMaximizeButtonVisiility)
        {
            if (this.MaximizeButton != null)
            {
                this.MaximizeButton.Visibility = maximizeButtonVisibility;
            }
            if (this.RestoreButton != null)
            {
                this.RestoreButton.Visibility = reverseMaximizeButtonVisiility;
            }
        }

        private void OpenSystemContextMenu(MouseButtonEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(this);
            System.Windows.Point screen = this.PointToScreen(position);
            int num = 36;
            if (position.Y < (double)num)
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                IntPtr systemMenu = NativeUtils.GetSystemMenu(handle, false);
                if (base.WindowState != WindowState.Maximized)
                {
                    NativeUtils.EnableMenuItem(systemMenu, 61488, 0);
                }
                else
                {
                    NativeUtils.EnableMenuItem(systemMenu, 61488, 1);
                }
                int num1 = NativeUtils.TrackPopupMenuEx(systemMenu, NativeUtils.TPM_LEFTALIGN | NativeUtils.TPM_RETURNCMD, Convert.ToInt32(screen.X + 2), Convert.ToInt32(screen.Y + 2), handle, IntPtr.Zero);
                if (num1 == 0)
                {
                    return;
                }

                NativeUtils.PostMessage(handle, 274, new IntPtr(num1), IntPtr.Zero);
            }
        }

        void IThemeSetter.SetTheme(bool isLight)
        {
            var windowBackground = isLight ? System.Windows.Application.Current.FindResource("Brushes.Light.Grey.Grey4Background") : System.Windows.Application.Current.FindResource("Brushes.Dark.Grey.Grey1Background");
            this.Background = windowBackground as SolidColorBrush;
        }

        protected bool HideIconAndTitle { get; set; } = false;
        private void HideIconAndTitleMethod()
        {
            if (NHMIcon != null) NHMIcon.Visibility = Visibility.Hidden;
            if (WindowTitle != null) WindowTitle.Visibility = Visibility.Hidden;
        }
    }
}
