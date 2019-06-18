using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;

namespace Guid.Providers
{

    /// <summary>
    /// Helper class used to generate the MD5 and SHA1 hashs used in V3 and V5 GUIDs
    /// </summary>
    /// <remarks>
    /// Uses the BouncyCastle project for support in the portable framework
    /// </remarks>
    public static class HashProvider
    {
        public static byte[] GenerateMD5Hash(byte[] namespaceId, byte[] data)
        {
            IDigest digest = new MD5Digest();
            digest.BlockUpdate(namespaceId, 0, namespaceId.Length);
            digest.BlockUpdate(data, 0, data.Length);
            var output = new byte[128];
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] GenerateSHA1Hash(byte[] namespaceId, byte[] data)
        {
            IDigest digest = new Sha1Digest();
            digest.BlockUpdate(namespaceId, 0, namespaceId.Length);
            digest.BlockUpdate(data, 0, data.Length);
            var output = new byte[128];
            digest.DoFinal(output, 0);
            return output;
        }
    }
}
