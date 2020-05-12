using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeamRedMiner
{
    // SGMiner API
    #region JSON Generated code
#pragma warning disable IDE1006 // Naming Styles
    public class ApiSTATUS
    {
        public string STATUS { get; set; }
        public int When { get; set; }
        public int Code { get; set; }
        public string Msg { get; set; }
        public string Description { get; set; }
    }

    public class ApiSUMMARY
    {
        public int Elapsed { get; set; }

        [JsonProperty("MHS av")]
        public double MHS_av { get; set; }

        [JsonProperty("MHS 5s")]
        public double MHS_5s { get; set; }

        [JsonProperty("KHS av")]
        public int KHS_av { get; set; }

        [JsonProperty("KHS 5s")]
        public int KHS_5s { get; set; }

        [JsonProperty("Found Blocks")]
        public int Found_Blocks { get; set; }

        public int Getworks { get; set; }
        public int Accepted { get; set; }
        public int Rejected { get; set; }

        [JsonProperty("Hardware Errors")]
        public int Hardware_Errors { get; set; }

        public double Utility { get; set; }
        public int Discarded { get; set; }
        public int Stale { get; set; }

        [JsonProperty("Get Failures")]
        public int Get_Failures { get; set; }

        [JsonProperty("Local Work")]
        public int Local_Work { get; set; }

        [JsonProperty("Remote Failures")]
        public int Remote_Failures { get; set; }

        [JsonProperty("Network Blocks")]
        public int Network_Blocks { get; set; }

        [JsonProperty("Total MH")]
        public double Total_MH { get; set; }

        [JsonProperty("Work Utility")]
        public double Work_Utility { get; set; }

        [JsonProperty("Difficulty Accepted")]
        public double Difficulty_Accepted { get; set; }

        [JsonProperty("Difficulty Rejected")]
        public double Difficulty_Rejected { get; set; }

        [JsonProperty("Difficulty Stale")]
        public double Difficulty_Stale { get; set; }

        [JsonProperty("Best Share")]
        public double Best_Share { get; set; }

        [JsonProperty("Device Hardware%")]
        public double Device_HardwarePerc { get; set; }

        [JsonProperty("Device Rejected%")]
        public double Device_RejectedPerc { get; set; }

        [JsonProperty("Pool Rejected%")]
        public double Pool_RejectedPerc { get; set; }

        [JsonProperty("Pool Stale%")]
        public double Pool_StalePerc { get; set; }

        [JsonProperty("Last getwork")]
        public int Last_getwork { get; set; }
    }

    // JSON API: {"command": "summary"}
    public class ApiSummaryRoot
    {
        public List<ApiSTATUS> STATUS { get; set; }
        public List<ApiSUMMARY> SUMMARY { get; set; }
        public int id { get; set; }
    }

    public class DEV
    {
        public int GPU { get; set; }
        public string Enabled { get; set; }
        public string Status { get; set; }
        public double Temperature { get; set; }

        //[JsonProperty("Fan Speed")]
        //public int Fan_Speed { get; set; }

        //[JsonProperty("Fan Percent")]
        //public int Fan_Percent { get; set; }

        //[JsonProperty("GPU Clock")]
        //public int GPU_Clock { get; set; }

        //[JsonProperty("Memory Clock")]
        //public int Memory_Clock { get; set; }

        [JsonProperty("GPU Voltage")]
        public double GPU_Voltage { get; set; }

        [JsonProperty("GPU Activity")]
        public int GPU_Activity { get; set; }

        public int Powertune { get; set; }

        //[JsonProperty("MHS av")]
        //public double MHS_av { get; set; }

        //[JsonProperty("MHS 5s")]
        //public double MHS_5s { get; set; }

        [JsonProperty("KHS av")]
        public double KHS_av { get; set; }

        [JsonProperty("KHS 5s")]
        public double KHS_5s { get; set; }

        public int Accepted { get; set; }
        public int Rejected { get; set; }

        [JsonProperty("Hardware_Errors")]
        public int Hardware_Errors { get; set; }

        //public double Utility { get; set; }
        //public string Intensity { get; set; }
        //public int XIntensity { get; set; }
        //public int RawIntensity { get; set; }

        //[JsonProperty("Last Share Pool")]
        //public int Last_Share_Pool { get; set; }

        //[JsonProperty("Last Share Time")]
        //public int Last_Share_Time { get; set; }

        //[JsonProperty("Total MH")]
        //public double Total_MH { get; set; }

        [JsonProperty("Diff1 Work")]
        public double Diff1_Work { get; set; }

        [JsonProperty("Difficulty Accepted")]
        public double Difficulty_Accepted { get; set; }

        [JsonProperty("Difficulty Rejected")]
        public double Difficulty_Rejected { get; set; }

        [JsonProperty("Last Share Difficulty")]
        public double Last_Share_Difficulty { get; set; }

        [JsonProperty("Last Valid Work")]
        public int Last_Valid_Work { get; set; }

        [JsonProperty("Device Hardware%")]
        public double Device_HardwarePerc { get; set; }

        [JsonProperty("Device Rejected%")]
        public double Device_RejectedPerc { get; set; }

        [JsonProperty("Device Elapsed")]
        public int Device_Elapsed { get; set; }
    }

    // JSON API: {"command": "devs"}
    public class ApiDevsRoot
    {
        public List<ApiSTATUS> STATUS { get; set; }
        public List<DEV> DEVS { get; set; }
        public int id { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
    #endregion JSON Generated code

}
