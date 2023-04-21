
namespace NHM.DeviceMonitoring.TDP
{
    public interface ITDP
    {
        TDPSettingType SettingType { get; set; }

        double TDPPercentage { get; }
        bool SetTDP(double percentage);
        TDPSimpleType TDPSimple { get; }
        bool SetTDPSimple(TDPSimpleType level);
    }
}
