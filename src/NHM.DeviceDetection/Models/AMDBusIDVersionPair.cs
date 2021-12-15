using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.Models.AMDBusIDVersionPair
{
    [Serializable]
    internal class AMDBusIDVersionPair
    {
        public string AdrenalinVersion { get; set; } = "NONE";
        public int BUS_ID { get; set; } = -1;
    }
}
