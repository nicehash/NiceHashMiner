using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MyDownloader.Core
{
    [Serializable]
    public class ResourceLocation
    {
        #region Fields
        
        private string url;
        private bool authenticate;
        private string login;
        private string password;
        private Type protocolProviderType;
        private IProtocolProvider provider;

        #endregion

        #region Constructor
        public ResourceLocation()
        {
        }

        public static ResourceLocation FromURL(string url)
        {
            ResourceLocation rl = new ResourceLocation();
            rl.URL = url;
            return rl;
        }

        public static ResourceLocation[] FromURLArray(string[] urls)
        {
            List<ResourceLocation> result = new List<ResourceLocation>();

            for (int i = 0; i < urls.Length; i++)
            {
                if (IsURL(urls[i]))
                {
                    result.Add(ResourceLocation.FromURL(urls[i]));
                }
            }

            return result.ToArray();
        }

        public static ResourceLocation FromURL(
            string url,
            bool authenticate,
            string login,
            string password)
        {
            ResourceLocation rl = new ResourceLocation();
            rl.URL = url;
            rl.Authenticate = authenticate;
            rl.Login = login;
            rl.Password = password;
            return rl;
        } 
        #endregion

        #region Properties

        public string URL
        {
            get { return url; }
            set 
            { 
                url = value;
                BindProtocolProviderType();
            }
        }

        public bool Authenticate
        {
            get { return authenticate; }
            set { authenticate = value; }
        }

        public string Login
        {
            get { return login; }
            set { login = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string ProtocolProviderType
        {
            get
            {
                if (protocolProviderType == null)
                {
                    return null;
                }
                return protocolProviderType.AssemblyQualifiedName;
            }
            set
            {
                if (value == null)
                {
                    BindProtocolProviderType();
                }
                else
                {
                    protocolProviderType = Type.GetType(value);
                }
            }
        }

        #endregion

        #region Methods

        public IProtocolProvider GetProtocolProvider(Downloader downloader)
        {
            return BindProtocolProviderInstance(downloader);
        }

        public void BindProtocolProviderType()
        {
            provider = null;

            if (! String.IsNullOrEmpty(this.URL))
            {
                protocolProviderType = ProtocolProviderFactory.GetProviderType(this.URL);                
            }            
        }

        public IProtocolProvider BindProtocolProviderInstance(Downloader downloader)
        {
            if (protocolProviderType == null)
            {
                BindProtocolProviderType();
            }

            if (provider == null)
            {
                provider = ProtocolProviderFactory.CreateProvider(protocolProviderType, downloader);
            }

            return provider;
        }

        public ResourceLocation Clone()
        {
            return (ResourceLocation)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return this.URL;
        }

        public static bool IsURL(string url)
        {
            Match m = Regex.Match(url, @"(?<Protocol>\w+):\/\/(?<Domain>[\w.]+\/?)\S*");
            if (m.ToString() != string.Empty)
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
