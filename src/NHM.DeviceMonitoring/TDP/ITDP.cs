
namespace NHM.DeviceMonitoring.TDP
{
    public interface ITDP
    {
        TDPSettingType SettingType { get; set; }

        // RAW values according to the underlying API
        double TDPRaw { get; }
        bool SetTDPRaw(double raw);

        double TDPPercentage { get; }
        bool SetTDPPercentage(double percentage);

        TDPSimpleType TDPSimple { get; }
        bool SetTDPSimple(TDPSimpleType level);
    }
}
