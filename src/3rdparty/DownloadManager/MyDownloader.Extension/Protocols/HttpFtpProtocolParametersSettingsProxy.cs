using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.Protocols
{
    class HttpFtpProtocolParametersSettingsProxy : IHttpFtpProtocolParameters
    {
        #region IHttpFtpProtocolParameters Members

        public string ProxyAddress
        {
            get
            {
                return Settings.Default.ProxyAddress;
            }
            set
            {
                Settings.Default.ProxyAddress = value;
            }
        }

        public string ProxyUserName
        {
            get
            {
                return Settings.Default.ProxyUserName;
            }
            set
            {
                Settings.Default.ProxyUserName = value;
            }
        }

        public string ProxyPassword
        {
            get
            {
                return Settings.Default.ProxyPassword;
            }
            set
            {
                Settings.Default.ProxyPassword = value;
            }
        }

        public string ProxyDomain
        {
            get
            {
                return Settings.Default.ProxyDomain;
            }
            set
            {
                Settings.Default.ProxyDomain = value;
            }
        }

        public bool UseProxy
        {
            get
            {
                return Settings.Default.UseProxy;
            }
            set
            {
                Settings.Default.UseProxy = value;
            }
        }

        public bool ProxyByPassOnLocal
        {
            get
            {
                return Settings.Default.ProxyByPassOnLocal;
            }
            set
            {
                Settings.Default.ProxyByPassOnLocal = value;
            }
        }

        public int ProxyPort
        {
            get
            {
                return Settings.Default.ProxyPort;
            }
            set
            {
                Settings.Default.ProxyPort = value;
            }
        }

        #endregion
    }
}
