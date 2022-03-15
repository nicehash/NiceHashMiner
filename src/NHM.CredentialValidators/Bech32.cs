/* Copyright (c) 2017 Pieter Wuille
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;

// https://github.com/DesWurstes/Bech32-Csharp/blob/master/Bech32-Csharp/Bech32-Csharp.cs
namespace Bech32_Csharp
{
    class Bech32ConversionException : Exception
    {
        public Bech32ConversionException()
            : base()
        {
        }
        public Bech32ConversionException(String message)
            : base(message)
        {
        }
    }
    public static class Converter
    {
        private const string CHARSET_BECH32 = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
        // https://play.golang.org/p/zZhIxabo-AQ
        private static readonly sbyte[] DICT_BECH32 = new sbyte[128]{
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            15, -1, 10, 17, 21, 20, 26, 30,  7,  5, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, 29, -1, 24, 13, 25,  9,  8, 23, -1, 18, 22, 31, 27, 19, -1,
             1,  0,  3, 16, 11, 28, 12, 14,  6,  4,  2, -1, -1, -1, -1, -1
        };
        private static uint PolyMod(byte[] input)
        {
            uint startValue = 1;
            for (uint i = 0; i < input.Length; i++)
            {
                uint c0 = startValue >> 25;
                startValue = (uint)(((startValue & 0x1ffffff) << 5) ^
                    (input[i]) ^
                    (-((c0 >> 0) & 1) & 0x3b6a57b2) ^
                    (-((c0 >> 1) & 1) & 0x26508e6d) ^
                    (-((c0 >> 2) & 1) & 0x1ea119fa) ^
                    (-((c0 >> 3) & 1) & 0x3d4233dd) ^
                    (-((c0 >> 4) & 1) & 0x2a1462b3));
            }
            return startValue ^ 1;
        }
        private static void hrpExpand(string hrp, byte[] ret)
        {
            int len = hrp.Length;
            //byte[] ret = new byte[len * 2 + 1];
            for (int i = 0; i < len; i++)
            {
                ret[i] = (byte)(hrp[i] >> 5);
            }
            ret[len] = 0;
            for (int i = 0; i < len; i++)
            {
                ret[len + 1 + i] = (byte)(hrp[i] & 31);
            }
        }
        private static byte[] createChecksum(string hrp, byte[] data, int dataLen)
        {
            int hrpLen = hrp.Length * 2 + 1;
            byte[] values = new byte[hrpLen + dataLen + 6];
            hrpExpand(hrp, values);
            System.Buffer.BlockCopy(data, 0, values, hrpLen, dataLen);
            uint mod = PolyMod(values);
            byte[] ret = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                ret[i] = (byte)((mod >> (5 * (5 - i))) & 31);
            }
            return ret;
        }
        private static bool verifyChecksum(string hrp, byte[] dataWithChecksum)
        {
            byte[] values = new byte[hrp.Length * 2 + 1 + dataWithChecksum.Length];
            hrpExpand(hrp, values);
            System.Buffer.BlockCopy(dataWithChecksum, 0, values, hrp.Length * 2 + 1, dataWithChecksum.Length);
            return PolyMod(values) == 0;
        }
        private static int convertBits(byte[] bytes, int inBits, uint outBits, bool pad, byte[] converted, int inPos, int outPos, int len)
        {
            //byte[] converted = new byte[bytes.Length * inBits / outBits + (bytes.Length * inBits % outBits != 0 ? 1 : 0)];
            uint bits = 0, maxv = (uint)((1 << (int)outBits) - 1), val = 0;
            //int len = bytes.Length - inPos;
            while (len-- != 0)
            {
                val = (val << inBits) | bytes[inPos++];
                bits += (uint)inBits;
                while (bits >= outBits)
                {
                    bits -= outBits;
                    converted[outPos++] = (byte)((val >> (int)bits) & maxv);
                }
            }
            if (pad)
            {
                if (bits != 0)
                {
                    converted[outPos++] = (byte)((val << (int)(outBits - bits)) & maxv);
                }
            }
            else if ((((val << (int)(outBits - bits)) & maxv) != 0) || bits >= inBits)
            {
                throw new Bech32ConversionException("Bit conversion error!" + bits + " " + inBits + " " + ((val << (int)(outBits - bits)) & maxv));
            }
            return outPos;
        }

        // Witness = witness version byte + 2-40 byte witness program
        // witness version can be only 0, currently
        // witness program is 20-byte RIPEMD160(SHA256(pubkey)) for P2WPKH
        // and 32-byte SHA256(script) for P2WSH
        // https://bitcoin.stackexchange.com/a/71219
        public static string EncodeBech32(byte witnessVersion, byte[] witnessProgram, bool isP2PKH, bool mainnet)
        {
            if (witnessProgram.Length < 3 || witnessProgram.Length > 41)
            {
                throw new Bech32ConversionException("Invalid witness program!");
            }
            System.Text.StringBuilder ret;
            byte[] data = new byte[80];
            int len = convertBits(witnessProgram, 8, 5, true, data, 0, 1, witnessProgram.Length);
            data[0] = witnessVersion;
            byte[] checksum;
            if (mainnet)
            {
                ret = new System.Text.StringBuilder("bc", 75); ;
                checksum = createChecksum("bc", data, len);
            }
            else
            {
                ret = new System.Text.StringBuilder("tb", 75); ;
                checksum = createChecksum("tb", data, len);
            }
            ret.Append('1');
            for (int i = 0; i < len; i++)
            {
                ret.Append((char)data[i]);
            }
            for (int i = 3, k = len + 3; i < k; i++)
            {
                ret[i] = CHARSET_BECH32[ret[i]];
            }
            for (int i = 0; i < 6; i++)
            {
                ret.Append(CHARSET_BECH32[checksum[i]]);
            }
            return ret.ToString();
        }
        // Returns the witnessProgram
        // isP2PKH is 0 for P2PKH, 1 for P2SH, 2 for "couldn't determine"
        public static byte[] DecodeBech32(string addr, out byte witnessVersion, out byte isP2PKH, out bool mainnet)
        {
            string addr2 = addr.ToLower();
            string hrp = "bc";
            if (addr2.StartsWith("bc1q"))
            {
                mainnet = true;
            }
            else if (addr2.StartsWith("tb1q"))
            {
                mainnet = false;
                hrp = "tb";
            }
            else
            {
                throw new Bech32ConversionException("Invalid Bech32 address!");
            }
            witnessVersion = 0;
            int dataLen = addr2.Length - 3;
            byte[] data = new byte[dataLen];
            for (int i = 0; i < dataLen; i++)
            {
                data[i] = (byte)addr2[3 + i];
            }
            sbyte err = 0;
            for (int i = 0; i < dataLen; i++)
            {
                sbyte k = DICT_BECH32[data[i]];
                err |= k;
                data[i] = unchecked((byte)k);
            }
            if (err == -1 || !verifyChecksum(hrp, data))
            {
                throw new Bech32ConversionException("Invalid Bech32 address!");
            }
            byte[] decoded = new byte[60];
            int decodedLen = convertBits(data, 5, 8, false, decoded, 1, 0, dataLen - 1 - 6);
            switch (decodedLen)
            {
                case 20:
                    isP2PKH = 0;
                    break;
                case 32:
                    isP2PKH = 1;
                    break;
                default:
                    isP2PKH = 2;
                    break;
            }
            byte[] final = new byte[decodedLen];
            System.Buffer.BlockCopy(decoded, 0, final, 0, decodedLen);
            return final;
        }
    }
}
