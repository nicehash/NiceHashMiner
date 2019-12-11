
namespace NHMCore.Nhmws.Models
{
#pragma warning disable 649, IDE1006
    class LoginMessage
    {
        public string method => "login";
        public string version { get; set; } = "";
        public int protocol { get; set; }

        public string btc { get; set; } = "";
        public string worker { get; set; } = "";
        public string group { get; set; } = "";
        public string rig { get; set; } = "";
    }
#pragma warning restore 649, IDE1006
}
