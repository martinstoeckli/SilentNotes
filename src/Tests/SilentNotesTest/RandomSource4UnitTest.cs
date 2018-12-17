using System;
using SilentNotes.Crypto;
using SilentNotes.Services;

namespace SilentNotesTest
{
    /// <summary>
    /// Implementation of the <see cref="ICryptoRandomSource"/> and <see cref="ICryptoRandomService"/>
    /// interface, for usage in UnitTests. Uses an unsafe random generator, so the system's entropy
    /// is not drained.
    /// </summary>
    public class RandomSource4UnitTest : ICryptoRandomService
    {
        private Random _nonCryptoRandom;
        private readonly byte _fixedByte;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomSource4UnitTest"/> class.
        /// It uses a non safe random generator to build the byte arrays.
        /// </summary>
        public RandomSource4UnitTest()
        {
            _nonCryptoRandom = new Random();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomSource4UnitTest"/> class.
        /// If builds byte arrays with fixed content.
        /// </summary>
        /// <param name="fixedByte">Value which is used to fill the "random" byte arrays with.</param>
        public RandomSource4UnitTest(byte fixedByte)
        {
            _fixedByte = fixedByte;
        }

        public byte[] GetRandomBytes(int numberOfBytes)
        {
            if (numberOfBytes <= 0)
                throw new ArgumentOutOfRangeException("numberOfBytes");

            byte[] result = new byte[numberOfBytes];

            if (_nonCryptoRandom != null)
            {
                // Use unsafe random
                _nonCryptoRandom.NextBytes(result);
            }
            else
            {
                // Fill with fixed values
                for (int index = 0; index < result.Length; index++)
                    result[index] = _fixedByte;
            }
            return result;
        }
    }
}
