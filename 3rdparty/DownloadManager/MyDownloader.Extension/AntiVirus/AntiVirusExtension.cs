using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using System.IO;
using System.Diagnostics;
using MyDownloader.Core;

namespace MyDownloader.Extension.AntiVirus
{
    public class AntiVirusExtension: IExtension
    {
        private IAntiVirusParameters parameters;

        #region Constructor
        
        public AntiVirusExtension():
            this(new AntiVirusParametersSettingsProxy())
        {  
        }

        public AntiVirusExtension(IAntiVirusParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            this.parameters = parameters;

            DownloadManager.Instance.DownloadEnded += new EventHandler<DownloaderEventArgs>(manager_DownloadEnded);
        }

        #endregion

        #region IExtension Members

        public string Name
        {
            get { return "Anti-virus integration"; }
        }

        public IUIExtension UIExtension
        {
            get
            {
                return new AntiVirusUIExtension();
            }
        }

        #endregion

        #region Methods

        void manager_DownloadEnded(object sender, MyDownloader.Core.DownloaderEventArgs e)
        {
            if (parameters.CheckFileWithAV)
            {
                string fileExtension = Path.GetExtension(e.Downloader.LocalFile).ToUpper();
                string[] extensionsToCheck = parameters.FileTypes.ToUpper().Split(';');
                int index = Array.IndexOf(extensionsToCheck, fileExtension);
                if (index >= 0)
                {
                    Process.Start(parameters.AVFileName,
                        String.Format(
                            "{0} {1}",
                            parameters.AVParameter,
                            e.Downloader.LocalFile));
                }
            }
        } 

        #endregion
    }
}
