using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.AutoDownloads
{
    internal class AutoDownloadsParametersSettingsProxy : IAutoDownloadsParameters
    {
        #region IAutoDownloadsParameters Members

        public int MaxJobs
        {
            get
            {
                return Settings.Default.MaxJobs;
            }
            set
            {
                Settings.Default.MaxJobs = value;
                OnParameterChanged("MaxJobs");
            }
        }

        public bool WorkOnlyOnSpecifiedTimes
        {
            get
            {
                return Settings.Default.WorkOnlyOnSpecifiedTimes;
            }
            set
            {
                Settings.Default.WorkOnlyOnSpecifiedTimes = value;
                OnParameterChanged("WorkOnlyOnSpecifiedTimes");
            }
        }

        public string TimesToWork
        {
            get
            {
                return Settings.Default.TimesToWork;
            }
            set
            {
                Settings.Default.TimesToWork = value;
                OnParameterChanged("TimesToWork");
            }
        }

        public double MaxRateOnTime
        {
            get
            {
                return Settings.Default.MaxRateOnTime;
            }
            set
            {
                Settings.Default.MaxRateOnTime = value;
                OnParameterChanged("MaxRateOnTime");
            }
        }

        public bool AutoStart
        {
            get
            {
                return Settings.Default.AutoStart;
            }
            set
            {
                Settings.Default.AutoStart = value;
                OnParameterChanged("AutoStart");
            }
        }

        #endregion

        #region IExtensionParameters Members

        public event System.ComponentModel.PropertyChangedEventHandler ParameterChanged;

        #endregion

        #region Methods

        protected void OnParameterChanged(string propertyname)
        {
            if (ParameterChanged != null)
            {
                ParameterChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyname));
            }
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnParameterChanged(e.PropertyName);
        }

        #endregion

        public AutoDownloadsParametersSettingsProxy()
        {
            Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
        }
    }
}
