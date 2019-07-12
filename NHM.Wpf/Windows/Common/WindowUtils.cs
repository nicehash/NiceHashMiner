using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                if (item is UIElement u)
                    Translate(u);
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
                    tb.Text = Tr(tb.Text);
                    break;
            }
        }

        public static void Translate(object o)
        {
            switch (o)
            {
                case UIElement u:
                    Translate(u);
                    break;
            }
        }
    }
}
