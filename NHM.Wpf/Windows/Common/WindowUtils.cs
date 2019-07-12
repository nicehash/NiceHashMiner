using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NHM.Wpf.ViewModels.Settings;
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

        public static void Translate(Decorator d)
        {
            Translate(d.Child);
        }

        public static void Translate(ItemsControl i)
        {
            foreach (var item in i.Items)
            {
                if (item is UIElement u)
                    Translate(u);
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
                    Translate(d);
                    break;
                case ContentControl c:
                    Translate(c);
                    break;
                case ItemsControl i:
                    Translate(i);
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
