using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using MyDownloader.Core;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using System.ComponentModel;

namespace MyDownloader.Extension.SpeedLimit
{
    public class SpeedLimitExtension: IExtension, IDisposable
    {
        private const int BalancerUp = 50;
        private const int BalancerDown = -75;

        private double currentWait;
        private bool enabled;
        private double maxLimit;
        private ISpeedLimitParameters parameters;

        #region IExtension Members

        public string Name
        {
            get { return "Speed Limit"; }
        }

        public IUIExtension UIExtension
        {
            get { return new SpeedLimitUIExtension(); }
        }

        #endregion

        #region Properties

        public ISpeedLimitParameters Parameters
        {
            get { return parameters; }
        }

        public bool CurrentEnabled
        {
            get { return enabled; }
        }

        public double CurrentMaxRate
        {
            get { return maxLimit; }
        }

        #endregion

        #region Methods

        public void SetMaxRateTemp(double max)
        {
            this.enabled = true;
            this.maxLimit = max;
        }

        public void RestoreMaxRateFromParameters()
        {
            ReadSettings();
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ReadSettings();
        }

        private void ReadSettings()
        {
            currentWait = 0;
            maxLimit = Parameters.MaxRate;
            enabled = Parameters.Enabled;
        }

        private void ProtocolProviderFactory_ResolvingProtocolProvider(object sender, ResolvingProtocolProviderEventArgs e)
        {
            e.ProtocolProvider = new ProtocolProviderProxy(e.ProtocolProvider, this);
        }

        internal void WaitFor()
        {
            if (enabled)
            {
                double totalRate = DownloadManager.Instance.TotalDownloadRate;

                if (totalRate > maxLimit)
                {
                    currentWait += BalancerUp;
                }
                else
                {
                    currentWait = Math.Max(currentWait + BalancerDown, 0);
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(currentWait));

                Debug.WriteLine("TotalDownloadRate = " + totalRate);
                Debug.WriteLine("maxLimit = " + maxLimit);
                Debug.WriteLine("currentWait = " + currentWait);
            }
        }

        #endregion

        #region Constructor

        public SpeedLimitExtension()
            :
            this(new SpeedLimitParametersSettingProxy())
        {
        }

        public SpeedLimitExtension(ISpeedLimitParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            this.parameters = parameters;

            ReadSettings();

            ProtocolProviderFactory.ResolvingProtocolProvider += new EventHandler<ResolvingProtocolProviderEventArgs>(ProtocolProviderFactory_ResolvingProtocolProvider);
            this.parameters.ParameterChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        } 

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this.parameters is IDisposable)
            {
                (this.parameters as IDisposable).Dispose();
            }
        }

        #endregion
    }
}