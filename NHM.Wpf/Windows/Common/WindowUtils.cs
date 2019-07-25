using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using NHM.Wpf.ViewModels.Settings;
using NHM.Wpf.Windows.Settings.Controls;
using static NHM.Wpf.Translations;

namespace NHM.Wpf.Windows.Common
{
    public static class WindowUtils
    {
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
                c.Content = Tr(s);
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
                        col.Header = Tr(s);
                }
            }
            else if (i is ListView lv && lv.View is GridView gv)
            {
                foreach (var col in gv.Columns)
                {
                    if (col.Header is string s)
                        col.Header = Tr(s);
                }
            }
        }

        public static void Translate(SettingsContainer sc)
        {
            foreach (var child in sc.Children)
            {
                Translate(child);
            }

            sc.Title = Tr(sc.Title);
            sc.Description = Tr(sc.Description);
        }

        public static void Translate(TextBlock tb)
        {
            if (!string.IsNullOrWhiteSpace(tb.Text) && tb.Inlines.Count <= 1)
                tb.Text = Tr(tb.Text);
            else
            {
                foreach (var inline in tb.Inlines)
                {
                    Translate(inline);
                }
            }
        }

        public static void Translate(Inline il)
        {
            if (il is Run run)
                run.Text = Tr(run.Text);
            else if (il is Span sp)
            {
                foreach (var inline in sp.Inlines)
                {
                    Translate(inline);
                }
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
            sb.Name = Tr(sb.Name);

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
    }
}
