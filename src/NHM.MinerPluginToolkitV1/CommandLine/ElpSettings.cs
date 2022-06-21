using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.CommandLine
{
    using Parameters = List<List<string>>;
    using DevicesParametersList = List<List<List<string>>>;
    public class ElpSettings
    {
        public Parameters MinerParameters { get; set; }
        public Parameters AlgorithmParameters { get; set; }
        public DevicesParametersList DevicesParametersList { get; set; }
    }
}
