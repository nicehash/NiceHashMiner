namespace NHM.Common.Enums
{
    // WARNING:
    // Names of this enum's values are in plaintext in `BenchmarkWindow.xaml` as part of data-binding for radio buttons
    // When renaming values, make sure to update these references as VS will not catch them
    public enum BenchmarkPerformanceType
    {
        Quick = 0,
        Standard,
        Precise
    }
}
