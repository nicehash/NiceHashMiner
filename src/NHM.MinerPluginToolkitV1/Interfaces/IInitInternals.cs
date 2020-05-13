
namespace NHM.MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IInitInternals interface is used by plugins to initialize all internal settings.
    /// Internal files are internal file settings that can be tweaked by the users.
    /// Most common settings are MinerOptionsPackage, MinerReservedPorts and MinerSystemEnvironmentVariables.
    /// </summary>
    public interface IInitInternals
    {
        void InitInternals();
    }
}
