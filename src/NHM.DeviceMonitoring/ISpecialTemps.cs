using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    public interface ISpecialTemps : IVramTemp, IHotspotTemp
    { }

    public interface IVramTemp 
    {
        int VramTemp { get; }
    }

    public interface IHotspotTemp 
    {
        int HotspotTemp { get; }
    }
}
