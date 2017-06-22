using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace MyDownloader.Extension.Zip
{
    public class ZipProtocolProvider: IProtocolProvider
    {
        private const string ZipEntryNameProperty = "ZipEntryNameProperty";

        #region Helper Methods

        public static void SetZipEntryNameProperty(Downloader downloader, string entryName)
        {
            downloader.ExtendedProperties[ZipEntryNameProperty] = entryName;
        }

        public static string GetZipEntryNameProperty(Downloader downloader)
        {
            return downloader.ExtendedProperties[ZipEntryNameProperty] as string;
        } 

        #endregion

        private Downloader downloader;
        private ZipRemoteFile remoteZipFile;
        private ZipEntry entry;

        #region IProtocolProvider Members

        public void Initialize(Downloader downloader)
        {
            this.downloader = downloader;
        }

        public Stream CreateStream(ResourceLocation rl, long initialPosition, long endPosition)
        {
            /*
             * a chamada desse metodo nao sera apos o GetFileInfo se o download for resumeable 
             * sendo assim, preciso ter algum repositorio comum para guardas os "remoteZipFile"
             * assim evita-se de ficar carregando as entries a cada segmento/arquivo.
             * 
             * outro, pronto, apos o uso do segmento, deve-se liberar o remoteZipFile se não houver
             * nenhum outro download vinculado a este "remoteZipFile"
             */
            return remoteZipFile.GetInputStream(entry);
        }

        public RemoteFileInfo GetFileInfo(ResourceLocation rl, out System.IO.Stream stream)
        {
            stream = null;

            remoteZipFile = new ZipRemoteFile(rl);
            remoteZipFile.Load();

            string entryName = GetZipEntryNameProperty(this.downloader);

            entry = remoteZipFile[entryName];

            RemoteFileInfo result = new RemoteFileInfo();
            result.AcceptRanges = false;  // TODO make resumeable
            result.FileSize = entry.Size;
            result.LastModified = entry.DateTime;
            result.MimeType = "application/zip";

            return result;
        }

        #endregion
    }
}
