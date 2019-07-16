using System;

namespace NHM.Wpf.ViewModels.Models.Placeholders
{
    public interface IStartupLoader
    {
        IProgress<(string, double)> PrimaryProgress { get; }
        IProgress<(string, double)> SecondaryProgress { get; }

        string PrimaryTitle { get; set; }
        string SecondaryTitle { get; set; }

        bool SecondaryVisible { get; set; }
    }
}
