using NHMCore.Configs;
using System.Collections.Generic;
using System.Windows;

namespace NHM.Wpf
{
    internal static class ThemeSetterManager
    {
        private static List<IThemeSetter> _themeSetters = new List<IThemeSetter>();
        private static bool IsLight = true;

        static ThemeSetterManager()
        {
            GUISettings.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private static void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GUISettings.DisplayTheme))
            {
                IsLight = GUISettings.Instance.DisplayTheme == "Light";
                SetTheme(IsLight);
            }
        }

        internal static void AddThemeSetter(IThemeSetter setter)
        {
            if (!_themeSetters.Contains(setter)) {
                _themeSetters.Add(setter);
                setter.SetTheme(IsLight);
            }
        }

        internal static void RemoveThemeSetter(IThemeSetter setter)
        {
            _themeSetters.Remove(setter);
        }

        internal static void SetThemeSelectedThemes()
        {
            SetTheme(IsLight);
        }

        internal static void SetTheme(bool isLight)
        {
            if (isLight)
            {
                
                // LIGHT
                Application.Current.Resources["BackgroundColor"] = Application.Current.FindResource("Brushes.Light.Grey.Grey4Background");
                Application.Current.Resources["BorderColor"] = Application.Current.FindResource("Brushes.Light.Border");
                //Application.Current.Resources["LoginCircle"] = Application.Current.FindResource("LoginCircleLogoLight");
                Application.Current.Resources["TextColorBrush"] = Application.Current.FindResource("Brushes.Light.TextColor");
            }
            else
            {
                // DARK
                Application.Current.Resources["BackgroundColor"] = Application.Current.FindResource("Brushes.Dark.Grey.Grey1Background");
                Application.Current.Resources["BorderColor"] = Application.Current.FindResource("Brushes.Dark.Border");
                //Application.Current.Resources["LoginCircle"] = Application.Current.FindResource("LoginCircleLogoDark");
                Application.Current.Resources["TextColorBrush"] = Application.Current.FindResource("Brushes.Dark.TextColor");
            }
            // Set the value
            foreach (var setter in _themeSetters)
            {
                setter.SetTheme(isLight);
            }
        }
    }
}
