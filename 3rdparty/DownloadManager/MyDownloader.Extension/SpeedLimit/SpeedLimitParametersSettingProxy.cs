using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.SpeedLimit
{
    internal class SpeedLimitParametersSettingProxy : ISpeedLimitParameters, IDisposable
    {
        #region ISpeedLimitParameters Members

        public bool Enabled
        {
            get
            {
                return Settings.Default.EnabledLimit;
            }
            set
            {
                Settings.Default.EnabledLimit = value;                
                OnParameterChanged("Enabled");
            }
        }

        public double MaxRate
        {
            get
            {
                return Settings.Default.MaxRate;
            }
            set
            {
                Settings.Default.MaxRate = value;
                OnParameterChanged("MaxRate");
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

        #region IDisposable Members

        public void Dispose()
        {
            Settings.Default.Save();
        }

        #endregion

        public SpeedLimitParametersSettingProxy()
        {
            Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
        }
    }
}
