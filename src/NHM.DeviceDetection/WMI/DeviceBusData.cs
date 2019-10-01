using System;
using System.Linq;

namespace NHM.DeviceDetection.WMI
{
    internal class DeviceBusData
    {
        public string Antecedent { get; set; }
        public string Dependent { get; set; }

        public int GetPCIBusID()
        {
            try
            {
                var pciBusStart = Antecedent.Substring(Antecedent.IndexOf("PCI"));
                var pciBusIDStr = pciBusStart
                                .Split('\\')
                                .Where(part => part.Contains("PCI_BUS"))
                                .Select(part => part.Replace("PCI_BUS", "").Trim('\\', '_', '"'))
                                .FirstOrDefault();
                if (int.TryParse(pciBusIDStr, out var pciBusID))
                {
                    return pciBusID;
                }
            }
            catch (Exception)
            {
                //throw;
            }

            return -1;
        }
    }
}
