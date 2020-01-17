using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NHM.Wpf.Views.Common.NHBase
{
    public abstract class NHMMainWindow : BaseDialogWindow
    {
        // the template style MUST mirror the enum names!!!
        protected enum ToggleButtonType
        {
            DashboardButton,
            DevicesButton,
            BenchmarkButton,
            PluginsButton,
            SettingsButton,
            HelpButton,
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
                Tabs[key] = tabButtom;
            }

            base.OnApplyTemplate();
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
    }
}
