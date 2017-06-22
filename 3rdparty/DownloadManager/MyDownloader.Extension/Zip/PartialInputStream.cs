using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace MyDownloader.Extension.Zip
{
    internal class PartialInputStream : InflaterInputStream
    {
        Stream baseStream;
        long filepos;
        long end;

        public PartialInputStream(Stream baseStream, long len)
            : base(baseStream)
        {
            this.baseStream = baseStream;
            filepos = 0;
            end = len;
        }

        public override int Available
        {
            get
            {
                long amount = end - filepos;
                if (amount > Int32.MaxValue)
                {
                    return Int32.MaxValue;
                }

                return (int)amount;
            }
        }

        public override int ReadByte()
        {
            if (filepos == end)
            {
                return -1;
            }

            lock (baseStream)
            {
                filepos++;
                return baseStream.ReadByte();
            }
        }

        public override int Read(byte[] b, int off, int len)
        {
            if (len > end - filepos)
            {
                len = (int)(end - filepos);
                if (len == 0)
                {
                    return 0;
                }
            }
            lock (baseStream)
            {
                int count = StreamHelper.ReadAll(b, off, len, baseStream);
                if (count > 0)
                {
                    filepos += len;
                }
                return count;
            }
        }

        public long SkipBytes(long amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (amount > end - filepos)
            {
                amount = end - filepos;
            }
            filepos += amount;
            for (int i = 0; i < amount; i++)
                baseStream.ReadByte();
            return amount;
        }

        public override void Close()
        {
            baseStream.Close();
        }
    }
}
