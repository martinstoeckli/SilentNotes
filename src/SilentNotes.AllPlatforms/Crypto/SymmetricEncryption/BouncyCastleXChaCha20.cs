// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace SilentNotes.Crypto.SymmetricEncryption
{
    /// <summary>
    /// Implementation of the <see cref="ISymmetricEncryptionAlgorithm"/> interface, implementing
    /// the XChaCha20-Poly1305 algorithm. Is is tested to be compatible with the libsodium library.
    /// This class depends on the BouncyCastle library.
    /// </summary>
    public class BouncyCastleXChaCha20 : ISymmetricEncryptionAlgorithm
    {
        /// <summary>The name of the XChaCha20-Poly1305 encryption algorithm.</summary>
        public const string CryptoAlgorithmName = "xchacha20_poly1305";

        private const int NonceSizeBytes = 24; // 192 bits
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
            if (ExpectedNonceSize != nonce.Length)
                throw new CryptoException("Invalid nonce size");

            ICipherParameters aeadParams = new AeadParameters(
                new KeyParameter(key), MacSizeBytes * 8, nonce, null);
            IAeadCipher chaCha20Poly1305 = new XChaCha20Poly1305();
            chaCha20Poly1305.Init(forEncryption, aeadParams);

            byte[] result = new byte[chaCha20Poly1305.GetOutputSize(data.Length)];
            int len = chaCha20Poly1305.ProcessBytes(data, 0, data.Length, result, 0);
            chaCha20Poly1305.DoFinal(result, len);
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
