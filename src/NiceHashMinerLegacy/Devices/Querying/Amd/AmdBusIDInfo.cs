namespace NiceHashMiner.Devices.Querying.Amd
{
    internal struct AmdBusIDInfo
    {
        public string Name { get; }
        public string Uuid { get; }
        public string InfSection { get; }
        public int Adl1Index { get; }
        public int Adl2Index { get; }

        public AmdBusIDInfo(string name, string uuid, string infSect, int adl1Indx, int adl2Indx)
        {
            Name = name;
            Uuid = uuid;
            InfSection = infSect;
            Adl1Index = adl1Indx;
            Adl2Index = adl2Indx;
        }
    }
}
