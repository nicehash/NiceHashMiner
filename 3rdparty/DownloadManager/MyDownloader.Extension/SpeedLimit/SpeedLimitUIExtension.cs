using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using System.Windows.Forms;
using MyDownloader.Extension.SpeedLimit.UI;

namespace MyDownloader.Extension.SpeedLimit
{
    public class SpeedLimitUIExtension: IUIExtension
    {
        #region IUIExtension Members

        public Control[] CreateSettingsView()
        {
            return new Control[] { new LimitCfg() };
        }

        public void PersistSettings(Control[] settingsView)
        {
            LimitCfg lmt = (LimitCfg)settingsView[0];

            Settings.Default.MaxRate = lmt.MaxRate;
            Settings.Default.EnabledLimit = lmt.EnableLimit;

            Settings.Default.Save();
        }

        #endregion

        public void ShowSpeedLimitDialog()
        {
            using (SetSpeedLimitDialog sld = new SetSpeedLimitDialog())
            {
                if (sld.ShowDialog() == DialogResult.OK)
                {
                    PersistSettings(new Control[] { sld.limitCfg1 });
                }
            }
        }
    }
}