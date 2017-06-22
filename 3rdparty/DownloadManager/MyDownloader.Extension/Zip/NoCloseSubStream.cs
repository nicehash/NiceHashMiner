using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyDownloader.Extension.Zip
{
    /// <summary>
    /// Stream without 
    /// </summary>
    internal class NoCloseSubStream : Stream
    {
        Stream baseStream;

        public NoCloseSubStream(Stream b)
        {
            baseStream = b;
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return baseStream.CanRead;
            }
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return baseStream.CanSeek;
            }
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return baseStream.CanWrite;
            }
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override long Length
        {
            get
            {
                return baseStream.Length;
            }
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override long Position
        {
            get
            {
                return baseStream.Position;
            }
            set
            {
                baseStream.Position = value;
            }
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStream.Seek(offset, origin);
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override void SetLength(long val)
        {
            baseStream.SetLength(val);
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override int ReadByte()
        {
            return baseStream.ReadByte();
        }

        /// <summary>
        /// I needed to implement the abstract member.
        /// </summary>
        public override int Read(byte[] b, int off, int len)
        {
            return baseStream.Read(b, off, len);
        }

        public override void Write(byte[] buf, int off, int len)
        {
            baseStream.Write(buf, off, len);
        }

        public override void WriteByte(byte bv)
        {
            baseStream.WriteByte(bv);
        }

        public override void Close()
        {
            baseStream = null;
        }

        public override void Flush()
        {
            baseStream.Flush();
        }
    }
}
