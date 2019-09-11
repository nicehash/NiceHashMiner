using System;

namespace NHM.Common
{
    public interface IStartupLoader
    {
        IProgress<(string, int)> PrimaryProgress { get; }
        IProgress<(string, int)> SecondaryProgress { get; }

        string PrimaryTitle { get; set; }
        string SecondaryTitle { get; set; }

        bool SecondaryVisible { get; set; }
    }
}
