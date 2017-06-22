using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Core
{
    public interface IMirrorSelector
    {
        void Init(Downloader downloader);

        ResourceLocation GetNextResourceLocation(); 
    }
}
