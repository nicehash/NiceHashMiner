using NHM.Common;
//using NiceHashMiner.ViewModels.Settings;
//using NiceHashMiner.Views.Settings.Controls;
using NHMCore;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;

namespace NiceHashMiner.Views.Common
{
    public static class WindowUtils
    {
        private const int GwlStyle = -16;
        private const int WsDisabled = 0x08000000;

        private const string UserDll = "user32.dll";

        [DllImport(UserDll)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport(UserDll)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Set the native disabled bit for the window.
        /// </summary>
        /// <param name="enabled">Whether bit set to enabled or disabled</param>
        /// <param name="winHandle">Handle to window</param>
        /// <returns>True iff successful</returns>
        /// <remarks>
        /// When the main window opens the loading init window, we want it to act like it does under ShowDialog().
        /// However, we need to use Show() instead. ShowDialog() not only disables the window controls, but also
        /// sets a native disabled bit in Windows that disallows the user to move the window.
        /// This function sets that same bit.
        /// </remarks>
        public static bool TrySetNativeEnabled(bool enabled, IntPtr winHandle)
        {
            try
            {
                var current = GetWindowLong(winHandle, GwlStyle);
                SetWindowLong(winHandle, GwlStyle, current & ~WsDisabled | (enabled ? 0 : WsDisabled));

                return true;
            }
            catch (Exception e)
            {
                Logger.Warn("WindowUtils", $"Set native window disabled failed: {e.Message}");

                return false;
            }
        }

        internal static bool ForceSoftwareRendering { get; set; } = true;

        internal static void SetForceSoftwareRendering(Window w)
        {
            if (ForceSoftwareRendering)
            {
                HwndSource hwndSource = PresentationSource.FromVisual(w) as HwndSource;
                HwndTarget hwndTarget = hwndSource.CompositionTarget;
                hwndTarget.RenderMode = RenderMode.SoftwareOnly;
            }
        }

        internal static double? Top = null;
        internal static double? Left = null;

        internal static void InitWindow(Window w)
        {
            if (Top.HasValue && Left.HasValue)
            {
                w.Top = Top.Value;
                w.Left = Left.Value;
            }
            else
            {
                w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            Translate(w);
        }

        internal static void Window_OnClosing(Window w)
        {
            Top = w.Top;
            Left = w.Left;
        }

        public static T AssertViewModel<T>(this FrameworkElement fe) where T : class, new()
        {
            if (!(fe.DataContext is T vm))
            {
                vm = new T();
                fe.DataContext = vm;
            }

            return vm;
        }

        #region Translations

        public static void Translate(Panel p)
        {
            if (p.ToolTip is string s)
            {
                p.ToolTip = Translations.Tr(s);
            }
            foreach (UIElement child in p.Children)
            {
                Translate(child);
            }
        }

        public static void Translate(ContentControl c)
        {
            if (c.Content is string s)
                c.Content = Translations.Tr(s);
            else if (c.Content is UIElement u)
                Translate(u);
        }

        public static void Translate(ItemsControl i)
        {
            foreach (var item in i.Items)
            {
                Translate(item);
            }

            if (i is DataGrid dg)
            {
                foreach (var col in dg.Columns)
                {
                    if (col.Header is string s)
                        col.Header = Translations.Tr(s);
                }
            }
            else if (i is ListView lv && lv.View is GridView gv)
            {
                foreach (var col in gv.Columns)
                {
                    if (col.Header is string s)
                        col.Header = Translations.Tr(s);
                }
            }
            //else if(i is TabControl tc) //TODO doesn't work
            //{
            //    foreach(var tab in tc.Items)
            //    {
            //        if (tab is TabItem ti)
            //        {
            //            ti.Header = Translations.Tr(ti.Header);
            //        }
            //    }

            //}
        }

        //public static void Translate(SettingsContainer sc)
        //{
        //    foreach (var child in sc.Children)
        //    {
        //        Translate(child);
        //    }

        //    sc.Title = Translations.Tr(sc.Title);
        //    sc.Description = Translations.Tr(sc.Description);
        //}

        public static void Translate(TextBlock tb)
        {
            foreach (var inline in EnumInlines(tb.Inlines))
            {
                Translate(inline);
            }
        }

        public static void Translate(Inline il)
        {
            if (il is Run run)
                run.Text = Translations.Tr(run.Text);
            else if (il is Span sp)
            {
                foreach (var inline in EnumInlines(sp.Inlines))
                {
                    Translate(inline);
                }
            }
        }

        private static IEnumerable<Inline> EnumInlines(InlineCollection inlineCollection)
        {
            // InlineCollection has the unfortunate trait that it will throw InvalidOperationException if
            // a child's text property is changed during a foreach. So this helper func iterates using
            // linked list properties of InlineCollection

            var il = inlineCollection.FirstInline;

            while (il != null)
            {
                yield return il;
                il = il.NextInline;
            }
        }

        public static void Translate(UIElement u)
        {
            switch (u)
            {
                case Panel p:
                    Translate(p);
                    break;
                case Decorator d:
                    Translate(d.Child);
                    break;
                //case SettingsContainer sc:
                //    Translate(sc);
                //    break;
                case ContentControl c:
                    Translate(c);
                    break;
                case ItemsControl i:
                    Translate(i);
                    break;
                case TextBlock tb:
                    Translate(tb);
                    break;
            }
        }

        //public static void Translate(SettingsBaseVM sb)
        //{
        //    sb.Name = Translations.Tr(sb.Name);

        //    foreach (var child in sb.Children)
        //    {
        //        Translate(child);
        //    }
        //}

        public static void Translate(object o)
        {
            switch (o)
            {
                case UIElement u:
                    Translate(u);
                    break;
                    //case SettingsBaseVM sb:
                    //    Translate(sb);
                    //    break;
            }
        }

        #endregion
    }
}
