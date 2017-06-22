using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using System.Windows.Forms;
using MyDownloader.Extension.AutoDownloads.UI;

namespace MyDownloader.Extension.AutoDownloads
{
    public class AutoDownloadsUIExtension: IUIExtension
    {
        #region IUIExtension Members

        public Control[] CreateSettingsView()
        {
            return new Control[] { new Jobs() };
        }

        public void PersistSettings(Control[] settingsView)
        {
            Jobs jobs = (Jobs)settingsView[0];

            Settings.Default.MaxJobs = jobs.MaxJobs;
            Settings.Default.WorkOnlyOnSpecifiedTimes = jobs.WorkOnlyOnSpecifiedTimes;
            Settings.Default.TimesToWork = jobs.TimesToWork;
            Settings.Default.MaxRateOnTime = jobs.MaxRate;
            Settings.Default.AutoStart = jobs.AutoStart;
            Settings.Default.Save();
        }

        #endregion
    }
}
