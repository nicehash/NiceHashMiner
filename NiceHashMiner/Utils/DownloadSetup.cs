namespace NiceHashMiner.Utils
{
    public class DownloadSetup
    {
        public DownloadSetup(string url, string dlName, string inFolderName)
        {
            BinsDownloadUrl = url;
            BinsZipLocation = dlName;
            ZipedFolderName = inFolderName;
        }

        public readonly string BinsDownloadUrl;
        public readonly string BinsZipLocation;
        public readonly string ZipedFolderName;
    }
}
