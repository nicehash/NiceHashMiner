using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MyDownloader.Core
{
    public static class ProtocolProviderFactory
    {
        private static Hashtable protocolHandlers = new Hashtable();

        public static event EventHandler<ResolvingProtocolProviderEventArgs> ResolvingProtocolProvider;

        public static void RegisterProtocolHandler(string prefix, Type protocolProvider)
        {
            protocolHandlers[prefix] = protocolProvider;
        }

        public static IProtocolProvider CreateProvider(string uri, Downloader downloader)
        {
            IProtocolProvider provider = InternalGetProvider(uri);

            if (downloader != null)
            {
                provider.Initialize(downloader);
            }

            return provider;
        }

        public static IProtocolProvider GetProvider(string uri)
        {
            return InternalGetProvider(uri);
        }

        public static Type GetProviderType(string uri)
        {
            int index = uri.IndexOf("://");

            if (index > 0)
            {
                string prefix = uri.Substring(0, index);
                Type type = protocolHandlers[prefix] as Type;
                return type;
            }
            else
            {
                return null;
            }
        }

        public static IProtocolProvider CreateProvider(Type providerType, Downloader downloader)
        {
            IProtocolProvider provider = CreateFromType(providerType);

            if (ResolvingProtocolProvider != null)
            {
                ResolvingProtocolProviderEventArgs e = new ResolvingProtocolProviderEventArgs(provider, null);
                ResolvingProtocolProvider(null, e);
                provider = e.ProtocolProvider;
            }

            if (downloader != null)
            {
                provider.Initialize(downloader);
            }

            return provider;
        }

        private static IProtocolProvider InternalGetProvider(string uri)
        {
            Type type = GetProviderType(uri);

            IProtocolProvider provider = CreateFromType(type);

            if (ResolvingProtocolProvider != null)
            {
                ResolvingProtocolProviderEventArgs e = new ResolvingProtocolProviderEventArgs(provider, uri);
                ResolvingProtocolProvider(null, e);
                provider = e.ProtocolProvider;
            }

            return provider;
        }

        private static IProtocolProvider CreateFromType(Type type)
        {
            IProtocolProvider provider = (IProtocolProvider)Activator.CreateInstance(type);
            return provider;
        }
    }
}
