// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace SilentNotes.Crypto.SymmetricEncryption
{
    /// <summary>
    /// Implementation of the <see cref="ISymmetricEncryptionAlgorithm"/> interface, implementing
    /// the TwoFish algorithm with the GCM mode. This is an authenticated encryption mode so there
    /// is no need to do additionally authentication afterwards.
    /// This class depends on the BouncyCastle library.
    /// </summary>
    public class BouncyCastleTwofishGcm : ISymmetricEncryptionAlgorithm
    {
        /// <summary>The name of the TwoFish encryption algorithm.</summary>
        public const string CryptoAlgorithmName = "twofish_gcm";

        // Using 128bits instead of 96 bits, will cause GCM to calculate a hash first and use 128
        // bits of the nonce. Since the nonce must not be reused, 96 bits offer too few
        // combinations to generate the nonce randomly (avoiding a stateful counter).
        private const int NonceSizeBytes = 16; // 128 bits instead of 96 bits
        private const int KeySizeBytes = 32; // 256 bits
        private const int MacSizeBytes = 16; // 128 bits

        /// <inheritdoc />
        public string Name
        {
            get { return CryptoAlgorithmName; }
        }

        /// <inheritdoc />
        public byte[] Encrypt(byte[] message, byte[] key, byte[] nonce)
        {
            return EncryptOrDecrypt(true, message, key, nonce);
        }

        /// <inheritdoc />
        public byte[] Decrypt(byte[] cipher, byte[] key, byte[] nonce)
        {
            return EncryptOrDecrypt(false, cipher, key, nonce);
        }

        private byte[] EncryptOrDecrypt(bool forEncryption, byte[] data, byte[] key, byte[] nonce)
        {
            if (ExpectedKeySize != key.Length)
                throw new CryptoException("Invalid key size");
            if (nonce.Length < 12) // A minimum of 96 bits is required
                throw new CryptoException("Invalid nonce size");

            ICipherParameters aeadParams = new AeadParameters(
                new KeyParameter(key), MacSizeBytes * 8, nonce, null);
            IAeadBlockCipher twofishGcm = new GcmBlockCipher(new TwofishEngine());
            twofishGcm.Init(forEncryption, aeadParams);

            byte[] result = new byte[twofishGcm.GetOutputSize(data.Length)];
            int len = twofishGcm.ProcessBytes(data, 0, data.Length, result, 0);
            twofishGcm.DoFinal(result, len);
            return result;
        }

        /// <inheritdoc />
        public int ExpectedKeySize
        {
            get { return KeySizeBytes; }
        }

        /// <inheritdoc />
        public int ExpectedNonceSize
        {
            get { return NonceSizeBytes; }
        }
    }
}
