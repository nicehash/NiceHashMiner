using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.BZip2;

namespace MyDownloader.Extension.Zip
{
    /// <summary>
    /// This is a FilterOutputStream that writes the files into a zip
    /// archive one after another.  It has a special method to start a new
    /// zip entry.  The zip entries contains information about the file name
    /// size, compressed size, CRC, etc.
    /// 
    /// It includes support for STORED and DEFLATED and BZIP2 entries.
    /// This class is not thread safe.
    /// 
    /// author of the original java version : Jochen Hoenicke
    /// </summary>
    /// <example> This sample shows how to create a zip file
    /// <code>
    /// using System;
    /// using System.IO;
    /// 
    /// using NZlib.Zip;
    /// 
    /// class MainClass
    /// {
    /// 	public static void Main(string[] args)
    /// 	{
    /// 		string[] filenames = Directory.GetFiles(args[0]);
    /// 		
    /// 		ZipOutputStream s = new ZipOutputStream(File.Create(args[1]));
    /// 		
    /// 		s.SetLevel(5); // 0 - store only to 9 - means best compression
    /// 		
    /// 		foreach (string file in filenames) {
    /// 			FileStream fs = File.OpenRead(file);
    /// 			
    /// 			byte[] buffer = new byte[fs.Length];
    /// 			fs.Read(buffer, 0, buffer.Length);
    /// 			
    /// 			ZipEntry entry = new ZipEntry(file);
    /// 			
    /// 			s.PutNextEntry(entry);
    /// 			
    /// 			s.Write(buffer, 0, buffer.Length);
    /// 			
    /// 		}
    /// 		
    /// 		s.Finish();
    /// 		s.Close();
    /// 	}
    /// }	
    /// </code>
    /// </example>
    internal class ZipOutputStream : DeflaterOutputStream
    {
        private ArrayList entries = new ArrayList();
        private Crc32 crc = new Crc32();
        private ZipEntry curEntry = null;

        private long startPosition = 0;
        private Stream additionalStream = null;

        private CompressionMethod curMethod;
        private int size;
        private int offset = 0;

        private byte[] zipComment = new byte[0];
        private int defaultMethod = DEFLATED;

        /// <summary>
        /// Our Zip version is hard coded to 1.0 resp. 2.0
        /// </summary>
        private const int ZIP_STORED_VERSION = 10;
        private const int ZIP_DEFLATED_VERSION = 20;

        /// <summary>
        /// Compression method.  This method doesn't compress at all.
        /// </summary>
        public const int STORED = 0;

        /// <summary>
        /// Compression method.  This method uses the Deflater.
        /// </summary>
        public const int DEFLATED = 8;

        public const int BZIP2 = 12;

        /// <summary>
        /// Creates a new Zip output stream, writing a zip archive.
        /// </summary>
        /// <param name="baseOutputStream">
        /// the output stream to which the zip archive is written.
        /// </param>
        public ZipOutputStream(Stream baseOutputStream)
            : base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, true))
        {
        }

        /// <summary>
        /// Set the zip file comment.
        /// </summary>
        /// <param name="comment">
        /// the comment.
        /// </param>
        /// <exception name ="ArgumentException">
        /// if UTF8 encoding of comment is longer than 0xffff bytes.
        /// </exception>
        public void SetComment(string comment)
        {
            byte[] commentBytes = ZipConstants.ConvertToArray(comment);
            if (commentBytes.Length > 0xffff)
            {
                throw new ArgumentException("Comment too long.");
            }
            zipComment = commentBytes;
        }

        /// <summary>
        /// Sets default compression method.  If the Zip entry specifies
        /// another method its method takes precedence.
        /// </summary>
        /// <param name = "method">
        /// the method.
        /// </param>
        /// <exception name = "ArgumentException">
        /// if method is not supported.
        /// </exception>
        public void SetMethod(int method)
        {
            if (method != STORED && method != DEFLATED && method != BZIP2)
            {
                throw new ArgumentException("Method not supported.");
            }
            defaultMethod = method;
        }

        /// <summary>
        /// Sets default compression level.  The new level will be activated
        /// immediately.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// if level is not supported.
        /// </exception>
        /// <see cref="Deflater"/>
        public void SetLevel(int level)
        {
            deflater_.SetLevel(level);
        }

        /// <summary>
        /// Write an unsigned short in little endian byte order.
        /// </summary>
        private void WriteLeShort(int value)
        {
            baseOutputStream_.WriteByte((byte)value);
            baseOutputStream_.WriteByte((byte)(value >> 8));
        }

        /// <summary>
        /// Write an int in little endian byte order.
        /// </summary>
        private void WriteLeInt(int value)
        {
            WriteLeShort(value);
            WriteLeShort(value >> 16);
        }

        /// <summary>
        /// Write an int in little endian byte order.
        /// </summary>
        private void WriteLeLong(long value)
        {
            WriteLeInt((int)value);
            WriteLeInt((int)(value >> 32));
        }


        bool shouldWriteBack = false;
        long seekPos = -1;
        /// <summary>
        /// Starts a new Zip entry. It automatically closes the previous
        /// entry if present.  If the compression method is stored, the entry
        /// must have a valid size and crc, otherwise all elements (except
        /// name) are optional, but must be correct if present.  If the time
        /// is not set in the entry, the current time is used.
        /// </summary>
        /// <param name="entry">
        /// the entry.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// if an I/O error occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// if stream was finished
        /// </exception>
        public void PutNextEntry(ZipEntry entry)
        {
            if (entries == null)
            {
                throw new InvalidOperationException("ZipOutputStream was finished");
            }

            if (curEntry != null)
            {
                CloseEntry();
            }

            CompressionMethod method = entry.CompressionMethod;
            int flags = 0;

            switch (method)
            {
                case CompressionMethod.Stored:
                    if (entry.CompressedSize >= 0)
                    {
                        if (entry.Size < 0)
                        {
                            entry.Size = entry.CompressedSize;
                        }
                        else if (entry.Size != entry.CompressedSize)
                        {
                            throw new ZipException("Method STORED, but compressed size != size");
                        }
                    }
                    else
                    {
                        entry.CompressedSize = entry.Size;
                    }

                    if (entry.Size < 0)
                    {
                        throw new ZipException("Method STORED, but size not set");
                    }
                    else if (entry.Crc < 0)
                    {
                        throw new ZipException("Method STORED, but crc not set");
                    }
                    break;
                case (CompressionMethod)12:
                    startPosition = baseOutputStream_.Position;
                    additionalStream = new BZip2OutputStream(new NoCloseSubStream(baseOutputStream_));
                    if (entry.CompressedSize < 0 || entry.Size < 0 || entry.Crc < 0)
                    {
                        flags |= 8;
                    }
                    break;
                case CompressionMethod.Deflated:
                    if (entry.CompressedSize < 0 || entry.Size < 0 || entry.Crc < 0)
                    {
                        flags |= 8;
                    }
                    break;
            }


            //			if (entry.DosTime < 0) {
            //				entry.Time = System.Environment.TickCount;
            //			}

            entry.Flags = flags;
            entry.Offset = offset;
            entry.CompressionMethod = (CompressionMethod)method;

            curMethod = method;
            // Write the local file header
            WriteLeInt(ZipConstants.LocalHeaderSignature);

            // write ZIP version
            WriteLeShort(method == CompressionMethod.Stored ? ZIP_STORED_VERSION : ZIP_DEFLATED_VERSION);
            if ((flags & 8) == 0)
            {
                WriteLeShort(flags);
                WriteLeShort((byte)method);
                WriteLeInt((int)entry.DosTime);
                WriteLeInt((int)entry.Crc);
                WriteLeInt((int)entry.CompressedSize);
                WriteLeInt((int)entry.Size);
            }
            else
            {
                if (baseOutputStream_.CanSeek)
                {
                    shouldWriteBack = true;
                    WriteLeShort((short)(flags & ~8));
                }
                else
                {
                    shouldWriteBack = false;
                    WriteLeShort(flags);
                }
                WriteLeShort((byte)method);
                WriteLeInt((int)entry.DosTime);
                seekPos = baseOutputStream_.Position;
                WriteLeInt(0);
                WriteLeInt(0);
                WriteLeInt(0);
            }
            byte[] name = ZipConstants.ConvertToArray(entry.Name);

            if (name.Length > 0xFFFF)
            {
                throw new ZipException("Name too long.");
            }
            byte[] extra = entry.ExtraData;
            if (extra == null)
            {
                extra = new byte[0];
            }
            if (extra.Length > 0xFFFF)
            {
                throw new ZipException("Extra data too long.");
            }

            WriteLeShort(name.Length);
            WriteLeShort(extra.Length);
            baseOutputStream_.Write(name, 0, name.Length);
            baseOutputStream_.Write(extra, 0, extra.Length);

            offset += ZipConstants.LocalHeaderBaseSize + name.Length + extra.Length;

            /* Activate the entry. */
            curEntry = entry;
            crc.Reset();
            if (method == CompressionMethod.Deflated)
            {
                deflater_.Reset();
            }
            size = 0;
        }

        /// <summary>
        /// Closes the current entry.
        /// </summary>
        /// <exception cref="System.IO.IOException">
        /// if an I/O error occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// if no entry is active.
        /// </exception>
        public void CloseEntry()
        {
            if (curEntry == null)
            {
                throw new InvalidOperationException("No open entry");
            }

            /* First finish the deflater, if appropriate */
            int csize = 0;
            if (curMethod == CompressionMethod.Deflated)
            {
                base.Finish();
                csize = (int)deflater_.TotalOut;
            }
            else if (curMethod == (CompressionMethod)12)
            {
                // close the sub stream, no problem because the substream has a fake
                // close
                additionalStream.Close();
                additionalStream = null;
                csize = (int)(baseOutputStream_.Position - startPosition);
            }
            else
                csize = size;

            if (curEntry.Size < 0)
            {
                curEntry.Size = size;
            }
            else if (curEntry.Size != size)
            {
                throw new ZipException("size was " + size + ", but I expected " + curEntry.Size);
            }

            if (curEntry.CompressedSize < 0)
            {
                curEntry.CompressedSize = csize;
            }
            else if (curEntry.CompressedSize != csize)
            {
                throw new ZipException("compressed size was " + csize + ", but I expected " + curEntry.CompressedSize);
            }

            if (curEntry.Crc < 0)
            {
                curEntry.Crc = crc.Value;
            }
            else if (curEntry.Crc != crc.Value)
            {
                throw new ZipException("crc was " + crc.Value +
                    ", but I expected " +
                    curEntry.Crc);
            }

            offset += csize;

            /* Now write the data descriptor entry if needed. */
            if (curMethod != CompressionMethod.Stored && (curEntry.Flags & 8) != 0)
            {
                if (shouldWriteBack)
                {
                    curEntry.Flags &= ~8;
                    long curPos = baseOutputStream_.Position;
                    baseOutputStream_.Seek(seekPos, SeekOrigin.Begin);
                    WriteLeInt((int)curEntry.Crc);
                    WriteLeInt((int)curEntry.CompressedSize);
                    WriteLeInt((int)curEntry.Size);
                    baseOutputStream_.Seek(curPos, SeekOrigin.Begin);
                    shouldWriteBack = false;
                }
                else
                {
                    WriteLeInt(ZipConstants.DataDescriptorSignature);
                    WriteLeInt((int)curEntry.Crc);
                    WriteLeInt((int)curEntry.CompressedSize);
                    WriteLeInt((int)curEntry.Size);
                    offset += ZipConstants.DataDescriptorSize;
                }
            }

            entries.Add(curEntry);
            curEntry = null;
        }

        /// <summary>
        /// Writes the given buffer to the current entry.
        /// </summary>
        /// <exception cref="System.IO.IOException">
        /// if an I/O error occured.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// if no entry is active.
        /// </exception>
        public override void Write(byte[] b, int off, int len)
        {
            if (curEntry == null)
            {
                throw new InvalidOperationException("No open entry.");
            }

            switch (curMethod)
            {
                case (CompressionMethod)12:
                    additionalStream.Write(b, off, len);
                    break;
                case CompressionMethod.Deflated:
                    base.Write(b, off, len);
                    break;
                case CompressionMethod.Stored:
                    baseOutputStream_.Write(b, off, len);
                    break;
            }

            crc.Update(b, off, len);
            size += len;
        }

        /// <summary>
        /// Finishes the stream.  This will write the central directory at the
        /// end of the zip file and flush the stream.
        /// </summary>
        /// <exception cref="System.IO.IOException">
        /// if an I/O error occured.
        /// </exception>
        public override void Finish()
        {
            if (entries == null)
            {
                return;
            }

            if (curEntry != null)
            {
                CloseEntry();
            }

            int numEntries = 0;
            int sizeEntries = 0;

            foreach (ZipEntry entry in entries)
            {
                // TODO : check the appnote file for compilance with the central directory standard
                CompressionMethod method = entry.CompressionMethod;
                WriteLeInt(ZipConstants.CentralHeaderSignature);
                WriteLeShort(method == CompressionMethod.Stored ? ZIP_STORED_VERSION : ZIP_DEFLATED_VERSION);
                WriteLeShort(method == CompressionMethod.Stored ? ZIP_STORED_VERSION : ZIP_DEFLATED_VERSION);
                if (entry.IsCrypted)
                {
                    entry.Flags |= 1;
                }
                WriteLeShort(entry.Flags);
                WriteLeShort((short)method);
                WriteLeInt((int)entry.DosTime);
                WriteLeInt((int)entry.Crc);
                WriteLeInt((int)entry.CompressedSize);
                WriteLeInt((int)entry.Size);

                byte[] name = ZipConstants.ConvertToArray(entry.Name);

                if (name.Length > 0xffff)
                {
                    throw new ZipException("Name too long.");
                }

                byte[] extra = entry.ExtraData;
                if (extra == null)
                {
                    extra = new byte[0];
                }

                string strComment = entry.Comment;
                byte[] comment = strComment != null ? ZipConstants.ConvertToArray(strComment) : new byte[0];
                if (comment.Length > 0xffff)
                {
                    throw new ZipException("Comment too long.");
                }

                WriteLeShort(name.Length);
                WriteLeShort(extra.Length);
                WriteLeShort(comment.Length);
                WriteLeShort(0); // disk number
                WriteLeShort(0); // internal file attr
                WriteLeInt(0);   // external file attr
                WriteLeInt((int)entry.Offset);

                baseOutputStream_.Write(name, 0, name.Length);
                baseOutputStream_.Write(extra, 0, extra.Length);
                baseOutputStream_.Write(comment, 0, comment.Length);
                ++numEntries;
                sizeEntries += ZipConstants.CentralHeaderBaseSize + name.Length + extra.Length + comment.Length;
            }

            WriteLeInt(ZipConstants.EndOfCentralDirectorySignature);
            WriteLeShort(0); // disk number 
            WriteLeShort(0); // disk with start of central dir
            WriteLeShort(numEntries);
            WriteLeShort(numEntries);
            WriteLeInt(sizeEntries);
            WriteLeInt(offset);
            WriteLeShort(zipComment.Length);
            baseOutputStream_.Write(zipComment, 0, zipComment.Length);
            baseOutputStream_.Flush();
            entries = null;
        }
    }
}
