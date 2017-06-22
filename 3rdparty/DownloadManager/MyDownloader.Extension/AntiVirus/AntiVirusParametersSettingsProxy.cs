using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.AntiVirus
{
    internal class AntiVirusParametersSettingsProxy : IAntiVirusParameters
    {
        #region IAntiVirusParameters Members

        public bool CheckFileWithAV
        {
            get
            {
                return Settings.Default.CheckFileWithAV;
            }
            set
            {
                Settings.Default.CheckFileWithAV = value;
            }
        }

        public string AVFileName
        {
            get
            {
                return Settings.Default.AVFileName;
            }
            set
            {
                Settings.Default.AVFileName = value;
            }
        }

        public string FileTypes
        {
            get
            {
                return Settings.Default.FileTypes;
            }
            set
            {
                Settings.Default.FileTypes = value;
            }
        }

        public string AVParameter
        {
            get
            {
                return Settings.Default.AVParameter;
            }
            set
            {
                Settings.Default.AVParameter = value;
            }
        }

        #endregion
    }
}
