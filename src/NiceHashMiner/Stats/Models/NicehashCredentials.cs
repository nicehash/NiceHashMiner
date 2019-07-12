
namespace NiceHashMiner.Stats.Models
{
    // TODO rename file to 'CredentialsSetMessage'
#pragma warning disable 649, IDE1006
    class NicehashCredentials
    {
        public string method => "credentials.set";
        public string btc { get; set; } = "";
        public string worker { get; set; } = "";
    }
#pragma warning restore 649, IDE1006
}
