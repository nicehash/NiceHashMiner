using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NiceHashMiner.Views.Common.NHBase
{
    public abstract class NHMMainWindow : BaseDialogWindow
    {

        Grid _gridLayoutRootOverlay;
        Grid _gridLayoutRootOverlay_MODAL_WINDOW_ContentGrid;
        Grid _gridLayoutRootOverlay_MODAL_WINDOW;
        ContentPresenter _contentPresenter;
        // the template style MUST mirror the enum names!!!
        protected enum ToggleButtonType
        {
            DashboardButton,
            DevicesButton,
            BenchmarkButton,
            PluginsButton,
            SettingsButton,
            NotificationsButton,
            //HelpButton,
        };

        private ToggleButtonType? _lastSelected;

        protected Dictionary<ToggleButtonType, ToggleButton> Tabs { get; private set; } = new Dictionary<ToggleButtonType, ToggleButton>();

        private bool HideInitTabButtonVisibility(string name)
        {
            if ("MinimizeButton" == name) return false;
            if ("CloseButton" == name) return false;
            return true;
        }

        protected abstract void OnTabSelected(ToggleButtonType tabType);

        public override void OnApplyTemplate()
        {
            foreach (var key in Enum.GetValues(typeof(ToggleButtonType)).Cast<ToggleButtonType>())
            {
                var name = key.ToString();
                var tabButtom = GetRequiredTemplateChild<ToggleButton>(name);
                if (tabButtom == null) throw new Exception($"Template Missing ToggleButton with name '{name}'. Make sure your Sytle template contains ToggleButton with name '{name}'.");
                tabButtom.Click += TabButtonButton_Click;
                tabButtom.IsEnabled = false;
                if (HideInitTabButtonVisibility(name))
                {
                    tabButtom.Visibility = Visibility.Hidden;
                }
                WindowUtils.Translate(tabButtom.Content);
                Tabs[key] = tabButtom;
            }
            _gridLayoutRootOverlay = GetRequiredTemplateChild<Grid>("LayoutRootOverlay");
            _gridLayoutRootOverlay_MODAL_WINDOW = GetRequiredTemplateChild<Grid>("MODAL_WINDOW_BLUR");
            _gridLayoutRootOverlay_MODAL_WINDOW_ContentGrid = GetRequiredTemplateChild<Grid>("MODAL_WINDOW_ContentGrid");

            _gridLayoutRootOverlay_MODAL_WINDOW_ContentGrid.MouseDown += _gridLayoutRootOverlay_MouseDown;
            _gridLayoutRootOverlay_MODAL_WINDOW.MouseDown += _gridLayoutRootOverlay_MouseDown;
            _gridLayoutRootOverlay.MouseDown += _gridLayoutRootOverlay_MouseDown;

            _contentPresenter = GetRequiredTemplateChild<ContentPresenter>("MODAL_DIALOG");
            if (_contentPresenter != null)
            {
                _contentPresenter.AddHandler(Grid.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnHeaderBarMouseLeftButtonDown));
                _contentPresenter.Content = null;
            }
            base.OnApplyTemplate();
        }

        public void SetBuildTag()
        {
            try
            {
                var buildTextBlock = GetRequiredTemplateChild<TextBlock>("BuildTagTextBlock");
                if (BuildOptions.BUILD_TAG != BuildTag.PRODUCTION && buildTextBlock != null) buildTextBlock.Text = BuildOptions.BUILD_TAG.ToString();
            }
            catch
            { }
        }

        private bool _isModalDialog = false;

        private void _gridLayoutRootOverlay_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isModalDialog) return;
            _gridLayoutRootOverlay.Visibility = Visibility.Hidden;
        }

        protected void SetTabButtonsEnabled()
        {
            foreach (var kvp in Tabs)
            {
                kvp.Value.IsEnabled = true;
                kvp.Value.Visibility = Visibility.Visible;
            }
            const ToggleButtonType initTab = ToggleButtonType.DashboardButton;
            _lastSelected = initTab;
            Tabs[initTab].IsChecked = true;
            OnTabSelected(initTab);
        }

        private void TabButtonButton_Click(object sender, RoutedEventArgs e)
        {
            // sender must be of ToggleButton Type
            var tabButton = (ToggleButton)sender;

            var currentKey = (ToggleButtonType)Enum.Parse(typeof(ToggleButtonType), tabButton.Name);
            if (_lastSelected == currentKey)
            {
                tabButton.IsChecked = true;
                return;
            }
            else
            {
                // select new 
                _lastSelected = currentKey;
                Tabs[currentKey].IsChecked = true;
                OnTabSelected(currentKey);
                // deselect other
                var deselectKeys = Tabs.Keys.Where(key => key != currentKey);
                foreach (var key in deselectKeys) Tabs[key].IsChecked = false;
            }
        }


#warning "YOU MUST MAKE SURE NOT TO DUPLICATE DIALOGS IN LOOPS"
        Queue<(UserControl uc, bool isModal)> _dialogsToShow = new Queue<(UserControl uc, bool isModal)>();
        public void ShowContentAsDialog(UserControl userControl)
        {
            if (_contentPresenter.Content == null)
            {
                _isModalDialog = false;
                _contentPresenter.Content = userControl;
                _gridLayoutRootOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                _dialogsToShow.Enqueue((userControl, false));
            }
        }

        public void ShowContentAsModalDialog(UserControl userControl)
        {
            if (_contentPresenter.Content == null)
            {
                _isModalDialog = true;
                _contentPresenter.Content = userControl;
                _gridLayoutRootOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                _dialogsToShow.Enqueue((userControl, true));
            }
        }

        public void HideModal()
        {
            _gridLayoutRootOverlay.Visibility = Visibility.Hidden;
            _isModalDialog = false;
            _contentPresenter.Content = null;
            if (_dialogsToShow.Count > 0)
            {
                var (uc, isModal) = _dialogsToShow.Dequeue();
                if (isModal)
                {
                    ShowContentAsModalDialog(uc);
                }
                else
                {
                    ShowContentAsDialog(uc);
                }
            }
        }

        public void SetNotificationCount(int count)
        {
            var notificationButton = GetRequiredTemplateChild<ToggleButton>("NotificationsButton");
            if (notificationButton != null)
            {
                if (count == 0)
                {
                    notificationButton.Style = this.FindResource("bellWindowStyle") as Style;
                    notificationButton.Content = "\uf0f3";
                }
                else
                {
                    notificationButton.Style = this.FindResource("local.WindowTabButtonNotification") as Style;
                    notificationButton.Content = count >= 100 ? ":D" : count.ToString();
                }
            }
        }

    }
}
