// RemoteZipFile.cs
// Copyright (C) 2003 Emanuele Ruffaldi
//
// ZipEntry parsing code taken from ZipFile.cs in SharpLibZip
// Copyright (C) 2001 Mike Krueger
//
// The original SharpLibZip code was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.BZip2;
using System.Collections;
using System.IO;
using System.Text;

using ICSharpCode.SharpZipLib.Checksums;
using MyDownloader.Core;

namespace MyDownloader.Extension.Zip
{
	public class ZipRemoteFile : IEnumerable
	{
        private ZipEntry[] entries;
        private ResourceLocation url;
        private int MaxFileOffset;
        private IProtocolProvider protocolProvider;

        public ZipRemoteFile(ResourceLocation url) :
            this(url, ProtocolProviderFactory.GetProvider(url.URL))
        {
        }

        public ZipRemoteFile(ResourceLocation url, IProtocolProvider protocolProvider)
		{
            if (protocolProvider == null)
            {
                throw new ArgumentNullException("protocolProvider");
            }

            this.url = url;
            this.protocolProvider = protocolProvider;
		}

		/*
		end of central dir signature  	4 bytes (0x06054b50)
		number of this disk 	2 bytes
		number of the disk with the start of the central directory 	2 bytes
		total number of entries in the central directory on this disk 	2 bytes
		total number of entries in the central directory 	2 bytes
		size of the central directory 	4 bytes
		offset of start of central directory
		with respect to the starting disk number 	4 bytes
		.ZIP file comment length 	2 bytes
		.ZIP file comment 	(variable size)
		 */

		/// <summary>
		/// TODO: case when the whole file is smaller than 64K
		/// TODO: handle non HTTP case
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public bool Load()
		{
			int CentralOffset, CentralSize;
			int TotalEntries;
			if(!FindCentralDirectory(out CentralOffset, out CentralSize, out TotalEntries))
				return false;

			MaxFileOffset = CentralOffset;

			// now retrieve the Central Directory
			entries = new ZipEntry[TotalEntries];

			//HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			//req.AddRange(CentralOffset, CentralOffset+CentralSize);
			//HttpWebResponse res = (HttpWebResponse)req.GetResponse();

			using (Stream s = protocolProvider.CreateStream(this.url, 
                CentralOffset, CentralOffset + CentralSize))
			{
				// code taken from SharpZipLib with modification for not seekable stream
				// and adjustement for Central Directory entry
				for (int i = 0; i < TotalEntries; i++) 
				{
					if (ReadLeInt(s) != ZipConstants.CentralHeaderSignature) 
					{
						throw new ZipException("Wrong Central Directory signature");
					}
					
					// skip 6 bytes: version made (W), version ext (W), flags (W)
					ReadLeInt(s);
					ReadLeShort(s);
					int method = ReadLeShort(s);
					int dostime = ReadLeInt(s);
					int crc = ReadLeInt(s);
					int csize = ReadLeInt(s);
					int size = ReadLeInt(s);
					int nameLen = ReadLeShort(s);
					int extraLen = ReadLeShort(s);
					int commentLen = ReadLeShort(s);				
					// skip 8 bytes: disk number start, internal file attribs, external file attribs (DW)
					ReadLeInt(s);
					ReadLeInt(s);
					int offset = ReadLeInt(s);
					
					byte[] buffer = new byte[Math.Max(nameLen, commentLen)];

                    StreamHelper.ReadAll(buffer, 0, nameLen, s);
					string name = ZipConstants.ConvertToString(buffer);
					
					ZipEntry entry = new ZipEntry(name);
					entry.CompressionMethod = (CompressionMethod)method;
					entry.Crc = crc & 0xffffffffL;
					entry.Size = size & 0xffffffffL;
					entry.CompressedSize = csize & 0xffffffffL;
					entry.DosTime = (uint)dostime;
					if (extraLen > 0) 
					{
						byte[] extra = new byte[extraLen];
                        StreamHelper.ReadAll(extra, 0, extraLen, s);
						entry.ExtraData = extra;
					}
					if (commentLen > 0) 
					{
                        StreamHelper.ReadAll(buffer, 0, commentLen, s);
						entry.Comment = ZipConstants.ConvertToString(buffer);
					}
					entry.ZipFileIndex = i;
					entry.Offset = offset;
					entries[i] = entry;
					OnProgress((i*100)/TotalEntries);
				}
			}

			OnProgress(100);
			
			return true;
		}

		/// <summary>
		/// OnProgress during Central Header loading
		/// </summary>
		/// <param name="pct"></param>
		public virtual void OnProgress(int pct)
		{
		
		}

		/// <summary>
		/// Checks if there is a local header at the current position in the stream and skips it
		/// </summary>
		/// <param name="baseStream"></param>
		/// <param name="entry"></param>
		void SkipLocalHeader(Stream baseStream, ZipEntry entry)
		{
			lock(baseStream) 
			{
				if (ReadLeInt(baseStream) != ZipConstants.LocalHeaderSignature) 
				{
					throw new ZipException("Wrong Local header signature");
				}
				
				Skip(baseStream, 10+12);
				int namelen = ReadLeShort(baseStream);
				int extralen = ReadLeShort(baseStream);
				Skip(baseStream, namelen+extralen);
			}
		}

		/// <summary>
		/// Finds the Central Header in the Zip file. We can minimize the number of requests and
		/// the bytes taken
		/// 
		/// Actually we do: 256, 1024, 65536
		/// </summary>
		/// <param name="baseurl"></param>
		/// <returns></returns>
		bool FindCentralDirectory(out int  Offset, out int Size, out int Entries)
		{
			int currentLength = 256;
			Entries = 0;
			Size = 0;
			Offset = -1;

			while(true)
			{
				//HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
				//req.AddRange(-(currentLength+22));
				//HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                // bb = new byte[res.ContentLength];
                // endSize = StreamHelper.ReadAll(bb, 0, (int)res.ContentLength, res.getResponseStream());

                int endSize;
                byte[] bb;

                using (BinaryReader headerStream = new BinaryReader(protocolProvider.CreateStream(
                    this.url, -(currentLength + 22), 0)))
                {
                    using (MemoryStream data = new MemoryStream())
                    {
                        int count = 256;
                        byte[] buffer = new byte[count];

                        do
                        {
                            int readCount = headerStream.Read(buffer, 0, count);
                            if (readCount == 0)
                            {
                                break;
                            }
                            data.Write(buffer, 0, readCount);
                        }
                        while (count > 0);

                        bb = data.ToArray();
                        endSize = (int)data.Length;
                    }
                }

				// scan for the central block. The position of the central block
				// is end-comment-22
				//<
				// 50 4B 05 06
				int pos = endSize-22;
				//int state = 0;
				while(pos >= 0)
				{
					if(bb[pos] == 0x50)
					{
						if(bb[pos+1] == 0x4b && bb[pos+2] == 0x05 && bb[pos+3] == 0x06)
							break; // found!!
						pos -= 4;
					}
					else
						pos --;
				}

				if(pos < 0)
				{
					if(currentLength == 65536)
						break;

					if(currentLength == 1024)
						currentLength = 65536;
					else if(currentLength == 256)
						currentLength = 1024;
					else
						break;
				}
				else
				{
					// found it!! so at offset pos+3*4 there is Size, and pos+4*4
					// BinaryReader is so elegant but now it's too much
					Size = MakeInt(bb, pos+12);
					Offset = MakeInt(bb, pos+16);
					Entries = MakeShort(bb, pos+10);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get a Stream for reading the specified entry
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		public Stream GetInputStream(ZipEntry entry)
		{
			if(entry.Size == 0)
				return null;

			if (entries == null) 
			{
				throw new InvalidOperationException("ZipFile has closed");
			}
			
			int index = (int)entry.ZipFileIndex;
			if (index < 0 || index >= entries.Length || entries[index].Name != entry.Name) 
			{
				throw new IndexOutOfRangeException();
			}
			
			// WARNING
			// should parse the Local Header to get the data address
			// Maximum Size of the Local Header is ... 16+64K*2
			//
			// So the HTTP request should ask for the big local header, but actually the
			// additional data is not downloaded.
			// Optionally use an additional Request to be really precise
			//HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baseUrl);

			int limit = (int)(entry.Offset+entry.CompressedSize+16+65536*2);
			if(limit >= MaxFileOffset)
				limit = MaxFileOffset-1; 
			//req.AddRange((int)entry.Offset, limit);
			//HttpWebResponse res = (HttpWebResponse)req.GetResponse();
			//Stream baseStream = res.GetResponseStream();

            Stream baseStream = protocolProvider.CreateStream(this.url, (int)entry.Offset, limit);

			// skips all the header
			SkipLocalHeader(baseStream, entries[index]);
			CompressionMethod method = entries[index].CompressionMethod;

			Stream istr = new PartialInputStream(baseStream, entries[index].CompressedSize);
			switch (method) 
			{
				case CompressionMethod.Stored:
					return istr;
				case CompressionMethod.Deflated:
					return new InflaterInputStream(istr, new Inflater(true));
				case (CompressionMethod)12:
					return new BZip2InputStream(istr);
				default:
					throw new ZipException("Unknown compression method " + method);
			}
        }

        #region Helper methods

        /// <summary>
		/// Read an unsigned short in little endian byte order.
		/// </summary>
		/// <exception name="System.IO.IOException">
		/// if a i/o error occured.
		/// </exception>
		/// <exception name="System.IO.EndOfStreamException">
		/// if the file ends prematurely
		/// </exception>
        private static int ReadLeShort(Stream s)
		{
			return s.ReadByte() | s.ReadByte() << 8;
		}
		
		/// <summary>
		/// Read an int in little endian byte order.
		/// </summary>
		/// <exception name="System.IO.IOException">
		/// if a i/o error occured.
		/// </exception>
		/// <exception name="System.IO.EndOfStreamException">
		/// if the file ends prematurely
		/// </exception>
        private static int ReadLeInt(Stream s)
		{
			return ReadLeShort(s) | ReadLeShort(s) << 16;
		}

		private static void Skip(Stream s, int n)
		{
			for(int i = 0; i < n; i++)
				s.ReadByte();
		}

		private static int MakeInt(byte [] bb, int pos)
		{
			return bb[pos+0]|(bb[pos+1]<<8)|(bb[pos+2]<<16)|(bb[pos+3]<<24);
		}

        private static int MakeShort(byte[] bb, int pos)
		{
			return bb[pos+0]|(bb[pos+1]<<8);
		}

        #endregion 

        public int Size
        {
            get { return entries == null ? 0 : entries.Length; }
        }

        public ZipEntry this[string entryName]
        {
            get 
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Name == entryName)
                    {
                        return entries[i];
                    }
                }

                throw new ArgumentException("entryName");
            }
        }

        public ZipEntry this[int index]
        {
            get { return entries[index]; }
        }

		/// <summary>
		/// Returns an IEnumerator of all Zip entries in this Zip file.
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			if (entries == null) 
			{
				throw new InvalidOperationException("ZipFile has closed");
			}
			
			return new ZipEntryEnumeration(entries);
		}

		class ZipEntryEnumeration : IEnumerator
		{
			ZipEntry[] array;
			int ptr = -1;
			
			public ZipEntryEnumeration(ZipEntry[] arr)
			{
				array = arr;
			}
			
			public object Current 
			{
				get 
				{
					return array[ptr];
				}
			}
			
			public void Reset()
			{
				ptr = -1;
			}
			
			public bool MoveNext() 
			{
				return (++ptr < array.Length);
			}
		}	
	}
}