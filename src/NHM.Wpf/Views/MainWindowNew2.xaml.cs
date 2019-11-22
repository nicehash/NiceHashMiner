using NHM.Wpf.ViewModels;
using NHM.Wpf.Views.Common;
using NHM.Wpf.Views.Common.NHBase;
using NHM.Wpf.Views.PluginsNew.PluginItem;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Configs.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindowNew2.xaml
    /// </summary>
    public partial class MainWindowNew2 : NHMMainWindow
    {
        private readonly MainVM _vm;

        public MainWindowNew2()
        {
            InitializeComponent();

            _vm = this.AssertViewModel<MainVM>();

            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);

            WindowUtils.InitWindow(this);

            LoadingBar.Visibility = Visibility.Visible;

            _vm.SetTheme(this);

            ConfigManager.GeneralConfig.PropertyChanged += (s,e) => {
                if (e.PropertyName == nameof(GeneralConfig.DisplayTheme))
                {
                    //_vm.SetTheme(this);
                    var themeSetters = FindVisualChildren<DependencyObject>(this).ToList(); //.Where(depObj => depObj is IThemeSetter).Cast<IThemeSetter>();
                    foreach (var ts in themeSetters)
                    {
                        //ts.SetTheme(_vm.Theme);
                    }
                }
            };
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);

            try
            {
                await _vm.InitializeNhm(LoadingBar.StartupLoader);
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                // Re-enable managed controls
                IsEnabled = true;
                SetTabButtonsEnabled();
            }
        }

        protected override void OnTabSelected(ToggleButtonType tabType)
        {
            var tabName = tabType.ToString();
            foreach (TabItem tab in MainTabs.Items)
            {
                if (tabName.Contains(tab.Name))
                {
                    MainTabs.SelectedItem = tab;
                    break;
                }
            }
        }
    }
}
