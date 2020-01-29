
using NHMCore.Utils;

namespace NHMCore.Mining.Plugins
{
    public class EthlargementIntegratedPlugin : Ethlargement.Ethlargement
    {
        public static EthlargementIntegratedPlugin Instance { get; } = new EthlargementIntegratedPlugin();

        public override string PluginUUID => "Ethlargement";

        public bool IsSystemElevated => Helpers.IsElevated;
        public bool SystemContainsSupportedDevicesNotSystemElevated => SystemContainsSupportedDevices && !Helpers.IsElevated;

        public override void InitInternals()
        {
            base.InitInternals();
            OnPropertyChanged(nameof(SystemContainsSupportedDevicesNotSystemElevated));
        }
    }
}
