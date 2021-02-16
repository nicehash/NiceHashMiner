using System;
using System.Collections.Generic;

namespace Excavator
{
    [Serializable]
    internal class JSONAlgorithm
    {
        public int id { get; set; }
        public string name { get; set; }
        public double speed { get; set; }
    }

    [Serializable]
    internal class Worker
    {
        public int worker_id { get; set; }
        public int device_id { get; set; }
        public string device_uuid { get; set; }
        public List<JSONAlgorithm> algorithms { get; set; }
    }

    [Serializable]
    internal class JsonApiResponse
    {
        public List<Worker> workers { get; set; }
        public int id { get; set; }
        public object error { get; set; }
    }
}
