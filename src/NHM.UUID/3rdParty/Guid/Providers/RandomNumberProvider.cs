using System;
using Org.BouncyCastle.Security;

namespace Guid.Providers
{
    /// <summary>
    /// Provide support for quicker pseudo-random numbers as well as support for cryptographically strong random numbers 
    /// </summary>
    public partial class RandomNumberProvider
    {
        private RandomNumberMode _mode { get; set; }
        public RandomNumberMode Mode { get { return _mode; } }

        private static Random _pseudoRandom = new Random();

        public RandomNumberProvider(RandomNumberMode mode = RandomNumberMode.Crypro)
        {
            _mode = mode;
        }

        public void FillBytes(byte[] bytes)
        {
            if (Mode == RandomNumberMode.Pseudo)
            {
                _pseudoRandom.NextBytes(bytes);
            }
            else
            {
                FillCryptoBytes(bytes);
            }
        }

        private SecureRandom random = new SecureRandom();

        private void FillCryptoBytes(byte[] bytes)
        {
            random.NextBytes(bytes); // generates 8 random bytes
        }
    }
}
