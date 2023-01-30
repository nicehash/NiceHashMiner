using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;

namespace NHM.Common
{
    public static class FileHelpers
    {
        public static string GetFileSHA256Checksum(string filePath)
        {
            try
            {
                using var sha256Hash = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = sha256Hash.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception e)
            {
                Logger.Error(nameof(FileHelpers), $"{nameof(GetFileSHA256Checksum)} error: {e.Message}");
                return null;
            }
        }

        internal static string GetURLFileSHA256Checksum(string url)
        {
            try
            {
                using var sha256Hash = SHA256.Create();
                using var httpClient = new HttpClient();
                var hash = sha256Hash.ComputeHash(httpClient.GetStreamAsync(url).Result);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception e)
            {
                Logger.Error(nameof(FileHelpers), $"{nameof(GetURLFileSHA256Checksum)} error: {e.Message}");
                return null;
            }
        }
    }
}
