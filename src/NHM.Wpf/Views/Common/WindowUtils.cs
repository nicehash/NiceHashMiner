using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using NHM.Wpf.ViewModels.Settings;
using NHM.Wpf.Views.Settings.Controls;
using NHMCore;

namespace NHM.Wpf.Views.Common
{
    public static class WindowUtils
    {
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
        }

        public static void Translate(SettingsContainer sc)
        {
            foreach (var child in sc.Children)
            {
                Translate(child);
            }

            sc.Title = Translations.Tr(sc.Title);
            sc.Description = Translations.Tr(sc.Description);
        }

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
                case SettingsContainer sc:
                    Translate(sc);
                    break;
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

        public static void Translate(SettingsBaseVM sb)
        {
            sb.Name = Translations.Tr(sb.Name);

            foreach (var child in sb.Children)
            {
                Translate(child);
            }
        }

        public static void Translate(object o)
        {
            switch (o)
            {
                case UIElement u:
                    Translate(u);
                    break;
                case SettingsBaseVM sb:
                    Translate(sb);
                    break;
            }
        }

        #endregion
    }
}
