using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.Windows.Settings.Pages
{
    public interface ISettingsPage
    {
        object DataContext { get; }
    }
}
