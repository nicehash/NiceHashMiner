﻿using NHM.Common;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static NHMCore.Translations;

namespace NHMCore.Mining.Plugins
{
    // cross referenced local and online
    public class PluginPackageInfoCR : NotifyChangedBase
    {
        public PluginPackageInfoCR(string pluginUUID)
        {
            // check auto update
            try
            {
                IsAutoUpdateEnabled = !File.Exists(Paths.MinerPluginsPath(pluginUUID, "disable_autoupdates"));
            }
            catch (Exception e)
            {
                Logger.Debug("PluginPackageInfoCR constructor", $"PluginPackageInfoCR {e.Message}");
            }
        }
        private PluginPackageInfo _onlineInfo { get; set; }
        public PluginPackageInfo OnlineInfo
        {
            get => _onlineInfo;
            set
            {
                _onlineInfo = value;
                OnPropertyChanged(nameof(OnlineInfo));
                CommonOnPropertyChanged();
            }
        }

        private PluginPackageInfo _localInfo;
        public PluginPackageInfo LocalInfo
        {
            get => _localInfo;
            set
            {
                _localInfo = value;
                OnPropertyChanged(nameof(LocalInfo));
                OnPropertyChanged(nameof(Installed));
                CommonOnPropertyChanged();
            }
        }

        private void CommonOnPropertyChanged()
        {
            OnPropertyChanged(nameof(HasNewerVersion));

            OnPropertyChanged(nameof(PluginDescription));
            OnPropertyChanged(nameof(PluginAuthor));
            OnPropertyChanged(nameof(SupportedDevicesAlgorithms));

            // online only but can be here
            OnPropertyChanged(nameof(MinerPackageURL));
            OnPropertyChanged(nameof(PluginPackageURL));

            OnPropertyChanged(nameof(PluginVersion));
            OnPropertyChanged(nameof(PluginName));
            OnPropertyChanged(nameof(PluginUUID));
        }

        public bool HasNewerVersion
        {
            get
            {
                var localVer = LocalInfo?.PluginVersion;
                var onlineVer = OnlineInfo?.PluginVersion;
                if (localVer == null || onlineVer == null) return false;
                if (onlineVer.Major > localVer.Major) return true;
                return onlineVer.Major == localVer.Major && onlineVer.Minor > localVer.Minor;
            }
        }

        public bool Installed
        {
            get
            {
                return LocalInfo != null;
            }
        }

        public int SupportedDeviceCount { get; set; } = 0;

        public bool HasSupportedDevices => SupportedDeviceCount > 0;

        public PluginPackageInfo GetInfoSource()
        {
            if (OnlineInfo == null && LocalInfo != null) return LocalInfo;
            if (OnlineInfo?.PluginVersion != null &&
                LocalInfo?.PluginVersion != null &&
                LocalInfo?.PluginVersion > OnlineInfo?.PluginVersion)
            {
                return LocalInfo;
            }
            return OnlineInfo;
        }
        // for plugins that we provide we can know what versions are and are not supported
        public bool CompatibleNHPluginVersion
        {
            get
            {
                var ver = OnlineInfo?.PluginVersion ?? null;
                var isNHPlugin = "info@nicehash.com" == OnlineInfo?.PluginAuthor;
                // 5 brings new interfaces + Eaglesong, 6 new algo Cuckaroom, 7 new algo GrinCuckatoo32
                // current supported major versions are 5-7 inclusive (older not supported)
                if (isNHPlugin && ver != null)
                {
                    return Checkers.IsMajorVersionSupported(ver.Major);
                }
                // here we assume it is compatible so allow install
                return true;
            }
        }

        public string CompatibilityIssueMessage
        {
            get
            {
                const string UpdateNHMMessage = "The latest online version of this plugin is not compatible with your current version of NiceHash miner. Please update NiceHash miner to the latest version.";
                const string UpdatePluginMessage = "This plugin is not compatible with NiceHash miner. Please update this plugin to the latest version.";
                if (CompatibleNHPluginVersion) return string.Empty;
                var major = OnlineInfo?.PluginVersion?.Major ?? -1;
                var maxVersion = Checkers.GetLatestSupportedVersion;
                return major > maxVersion ? Tr(UpdateNHMMessage) : Tr(UpdatePluginMessage);
            }
        }

        public bool NHMNeedsUpdate
        {
            get
            {
                if (CompatibleNHPluginVersion) return false;
                var major = OnlineInfo?.PluginVersion?.Major ?? -1;
                var maxVersion = Checkers.GetLatestSupportedVersion;
                return major > maxVersion;
            }
        }

        // PluginPackageInfo region
        public string PluginUUID
        {
            get
            {
                var uuid = LocalInfo?.PluginUUID ?? OnlineInfo?.PluginUUID ?? "N/A";
                return uuid;
            }
        }

        public string PluginPackageHash
        {
            get
            {
                var hash = OnlineInfo?.PluginPackageHash ?? "N/A";
                return hash;
            }
        }

        public string BinaryPackageHash
        {
            get
            {
                var hash = OnlineInfo?.BinaryPackageHash ?? "N/A";
                return hash;
            }
        }

        public string PluginName
        {
            get
            {
                var name = LocalInfo?.PluginName ?? OnlineInfo?.PluginName ?? "N/A";
                return name;
            }
        }


        public Version PluginVersion
        {
            get
            {
                var ver = GetInfoSource()?.PluginVersion ?? new Version(0, 0);
                return ver;
            }
        }

        public string PluginPackageURL
        {
            get
            {
                var pluginURL = GetInfoSource()?.PluginPackageURL ?? "N/A";
                return pluginURL;
            }
        }

        public string MinerPackageURL
        {
            get
            {
                var minerURL = GetInfoSource()?.MinerPackageURL ?? "N/A";
                return minerURL;
            }
        }

        public Dictionary<string, List<string>> SupportedDevicesAlgorithms
        {
            get
            {
                var supportedDevicesAlgorithms = GetInfoSource()?.SupportedDevicesAlgorithms ?? new Dictionary<string, List<string>>();
                var keysWithNoAlgorithms = supportedDevicesAlgorithms
                    .Where(pair => !pair.Value.Any())
                    .Select(pair => pair.Key)
                    .Where(key => key != null)
                    .ToArray();
                foreach (var removeKey in keysWithNoAlgorithms) supportedDevicesAlgorithms.Remove(removeKey);
                return supportedDevicesAlgorithms;
            }
        }

        public string PluginAuthor
        {
            get
            {
                var author = GetInfoSource()?.PluginAuthor ?? "N/A";
                return author;
            }
        }


        public string PluginDescription
        {
            get
            {
                var desc = GetInfoSource()?.PluginDescription ?? "N/A";
                return desc;
            }
        }

        // TODO this shouldn't be here
        private bool _isAutoUpdateEnabled = true;
        public bool IsAutoUpdateEnabled
        {
            get
            {
                return _isAutoUpdateEnabled;
            }
            set
            {
                _isAutoUpdateEnabled = value;
                try
                {
                    var disableAutoUpdatesFile = Paths.MinerPluginsPath(PluginUUID, "disable_autoupdates");
                    if (value)
                    {
                        File.Delete(disableAutoUpdatesFile);
                    }
                    else
                    {
                        File.Create(disableAutoUpdatesFile);
                    }
                }
                catch (Exception)
                {
                }
                OnPropertyChanged(nameof(IsAutoUpdateEnabled));
            }
        }

        public string BinsPackagePassword
        {
            get
            {
                return OnlineInfo?.PackagePassword ?? null;
            }
        }

        public bool _isUserActionRequired = false;

        public bool IsUserActionRequired
        {
            get => _isUserActionRequired;
            set
            {
                _isUserActionRequired = value;
                OnPropertyChanged(nameof(IsUserActionRequired));
            }
        }
    }
}
