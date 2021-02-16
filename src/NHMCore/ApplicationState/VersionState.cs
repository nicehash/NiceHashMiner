using NHM.Common;
using NHMCore.Utils;
using System;
using System.Windows.Forms;

namespace NHMCore.ApplicationState
{
    public class VersionState : NotifyChangedBase
    {
        public static VersionState Instance { get; } = new VersionState();

        public const string UpdaterUrlTemplate = "https://github.com/nicehash/NiceHashMiner/releases/download/{VERSION_TAG}/nhm_windows_updater_{VERSION_TAG}.exe";
        public const string ZipUrlTemplate = "https://github.com/nicehash/NiceHashMiner/releases/download/{VERSION_TAG}/nhm_windows_{VERSION_TAG}.zip";

        //public const string UpdaterUrlTemplate = "https://github.com/nicehash/NiceHashMiner3Alpha/releases/download/{VERSION_TAG}/nhm_windows_updater_{VERSION_TAG}.exe";
        //public const string ZipUrlTemplate = "https://github.com/nicehash/NiceHashMiner3Alpha/releases/download/{VERSION_TAG}/nhm_windows_{VERSION_TAG}.zip";

        private VersionState()
        {
            ProgramVersion = new Version(Application.ProductVersion);
        }

        public Version ProgramVersion { get; private set; }
        public string OnlineVersionStr { get; private set; } = null;

        private Version _onlineVersion = null;
        public Version OnlineVersion
        {
            get => _onlineVersion;
            internal set
            {
                _onlineVersion = value;
                OnPropertyChanged(nameof(OnlineVersion));
                OnPropertyChanged(nameof(IsNewVersionAvailable));
            }
        }

        public bool IsNewVersionAvailable
        {
            get
            {
                try
                {
                    var ret = ProgramVersion.CompareTo(OnlineVersion);
                    // not sure why BetaAlphaPostfixString is being checked
                    var hasNewVersion = ret < 0;
                    // TODO hook on property changed here
                    //var displayNewVer = string.Format(Translations.Tr("IMPORTANT! New version v{0} has\r\nbeen released. Click here to download it."), version);
                    // display new version
                    // notify all components
                    //DisplayVersion?.Invoke(null, displayNewVer); // TODO broken make VersionUpdatesState
                    return hasNewVersion;
                }
                catch (Exception e)
                {
                    Logger.Error("VersionState", $"IsNewVersionAvailable exception: '{e}'");
                }
                return false;
            }
        }



        public void OnVersionUpdate(string version)
        {
            // update version
            if (OnlineVersionStr != version)
            {
                OnlineVersionStr = version;
            }
            if (OnlineVersionStr == null)
            {
                return;
            }
            try
            {
                var setOnline = new Version(OnlineVersionStr);
                OnlineVersion = setOnline;
            }
            catch (Exception e)
            {
                Logger.Error("VersionState", $"OnVersionUpdate trying to set '{version}' exception: '{e}'");
            }
        }

        public void VisitNewVersionUrl()
        {
            // let's not throw anything if online version is missing just go to releases
            var url = Links.VisitReleasesUrl;
            if (OnlineVersion != null)
            {
                url = Links.VisitNewVersionReleaseUrl + OnlineVersion;
            }
            Helpers.VisitUrlLink(url);
        }

        public string GetNewVersionUpdaterUrl()
        {
            var template = UpdaterUrlTemplate;
            var url = "";
            if (OnlineVersionStr != null)
            {
                url = template.Replace("{VERSION_TAG}", OnlineVersionStr);
            }
            return url;
        }

        public string GetNewVersionZipUrl()
        {
            var template = ZipUrlTemplate;
            var url = "";
            if (OnlineVersionStr != null)
            {
                url = template.Replace("{VERSION_TAG}", OnlineVersionStr);
            }
            return url;
        }
    }
}
