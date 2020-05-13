using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1.ClaymoreCommon
{
#pragma warning disable IDE1006 // Naming Styles
    public class JsonApiResponse
    {
        public List<string> result { get; set; }
        public int id { get; set; }
        public object error { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
