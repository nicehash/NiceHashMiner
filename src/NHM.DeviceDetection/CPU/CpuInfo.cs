namespace NHM.DeviceDetection.CPU
{
    internal record CpuInfo
    {
        public string VendorID;
        public string Family;
        //public string Model;
        public string PhysicalID;
        public string ModelName;
        public int NumberOfCores;
    }
}
