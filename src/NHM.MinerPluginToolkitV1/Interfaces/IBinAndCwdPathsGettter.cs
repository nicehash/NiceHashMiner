namespace NHM.MinerPluginToolkitV1.Interfaces
{
    // TODO change docs. Now we use this in PluginBase
    /// <summary>
    /// IBinAndCwdPathsGettter interface is used by plugins to define their binary and (current) working directory paths
    /// All <see cref="MinerBase"/> plugins must implement this interface. It is used in combination with <see cref="IBinaryPackageMissingFilesChecker"/>.
    /// </summary>
    public interface IBinAndCwdPathsGettter
    {
        (string binPath, string cwdPath) GetBinAndCwdPaths();
    }
}
