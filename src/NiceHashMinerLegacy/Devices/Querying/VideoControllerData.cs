namespace NiceHashMiner.Devices.Querying
{
    internal class VideoControllerData
    {
        public string Name { get; }
        public string Description { get; }
        public string PnpDeviceID { get; }
        public string DriverVersion { get; }
        public string Status { get; }
        public string InfSection { get; private set; } // get arhitecture
        public ulong AdapterRam { get; }

        public bool IsNvidia { get; }
        public bool IsAmd { get; }

        public VideoControllerData(string name,
            string desc,
            string pnpID,
            string driverVersion,
            string status,
            string infSection,
            ulong ram)
        {
            Name = name;
            Description = desc;
            PnpDeviceID = pnpID;
            DriverVersion = driverVersion;
            Status = status;
            InfSection = infSection;
            AdapterRam = ram;

            var lowerName = name.ToLower();

            IsNvidia = lowerName.Contains("nvidia");
            IsAmd = lowerName.Contains("amd") || lowerName.Contains("radeon") || lowerName.Contains("firepro");
        }

        public void SetInfSectionEmptyIfNull()
        {
            if (InfSection == null) InfSection = "";
        }

        public string GetFormattedString()
        {
            return $"\t\tName: {Name}\n" +
                   $"\t\tDescription: {Description}\n" +
                   $"\t\tPNPDeviceID: {PnpDeviceID}\n" +
                   $"\t\tDriverVersion: {DriverVersion}\n" +
                   $"\t\tStatus: {Status}\n" +
                   $"\t\tInfSection: {InfSection}\n" +
                   $"\t\tAdapterRAM: {AdapterRam}";
        }
    }
}
