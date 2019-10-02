
namespace NHMCore.Mining.Plugins
{
    public class EthlargementIntegratedPlugin : Ethlargement.Ethlargement
    {
        public static EthlargementIntegratedPlugin Instance { get; } = new EthlargementIntegratedPlugin();

        public override string PluginUUID => "Ethlargement";
    }
}
