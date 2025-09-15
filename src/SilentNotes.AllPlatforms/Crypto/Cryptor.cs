﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Security;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Workers;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Implementation of the <see cref="ICryptor"/> interface.
    /// </summary>
    public class Cryptor : ICryptor
    {
        /// <summary>Can be used in the compression parameter, to compress with the GZip library.</summary>
        public const string CompressionGzip = "gzip";
        private const int MinPasswordLength = 5;
        private readonly ICryptoRandomSource _randomSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cryptor"/> class.
        /// </summary>
        /// <param name="packageName">Sets the <see cref="PackageName"/> property.</param>
        /// <param name="randomSource">A cryptographically safe random generator. It is required to
        /// encrypt data, if the cryptor is exclusively used for decryption, null can be passed.</param>
        public Cryptor(string packageName, ICryptoRandomSource randomSource)
        {
            PackageName = packageName;
            _randomSource = randomSource;
        }

        /// <inheritdoc/>
        public string PackageName { get; private set; }

        /// <inheritdoc/>
        public byte[] Encrypt(
            byte[] message,
            SecureString password,
            KeyDerivationCostType costType, 
            string encryptorName,
            string kdfName,
            string compression = null)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            ValidatePassword(password);
            if (_randomSource == null)
                throw new ArgumentNullException(nameof(_randomSource));
            if (string.IsNullOrWhiteSpace(encryptorName))
                encryptorName = BouncyCastleXChaCha20.CryptoAlgorithmName;
            if (string.IsNullOrWhiteSpace(kdfName))
                encryptorName = Pbkdf2.CryptoKdfName;
            ISymmetricEncryptionAlgorithm encryptor = new SymmetricEncryptionAlgorithmFactory().CreateAlgorithm(encryptorName);
            IKeyDerivationFunction kdf = new KeyDerivationFactory().CreateKdf(kdfName);

            // Prepare header
            CryptoHeader header = new CryptoHeader();
            header.PackageName = PackageName;
            header.AlgorithmName = encryptor.Name;
            header.Nonce = _randomSource.GetRandomBytes(encryptor.ExpectedNonceSize);
            header.KdfName = kdf.Name;
            header.Salt = _randomSource.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            header.Cost = kdf.RecommendedCost(costType);
            header.Compression = compression;

            try
            {
                if (string.Equals(CompressionGzip, header.Compression, StringComparison.InvariantCultureIgnoreCase))
                    message = CompressUtils.Compress(message);

                byte[] key = kdf.DeriveKeyFromPassword(password, encryptor.ExpectedKeySize, header.Salt, header.Cost);
                byte[] cipher = encryptor.Encrypt(message, key, header.Nonce);
                return CryptoHeaderPacker.PackHeaderAndCypher(header, cipher);
            }
            catch (Exception ex)
            {
                throw new CryptoException("An unexpected error occured, while encrypting the message.", ex);
            }
        }

        /// <inheritdoc/>
        public byte[] Encrypt(
            byte[] message,
            byte[] key,
            string encryptorName,
            string compression = null)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (_randomSource == null)
                throw new ArgumentNullException(nameof(_randomSource));
            if (string.IsNullOrWhiteSpace(encryptorName))
                encryptorName = BouncyCastleXChaCha20.CryptoAlgorithmName;
            ISymmetricEncryptionAlgorithm encryptor = new SymmetricEncryptionAlgorithmFactory().CreateAlgorithm(encryptorName);

            // Prepare header
            CryptoHeader header = new CryptoHeader();
            header.PackageName = PackageName;
            header.AlgorithmName = encryptor.Name;
            header.Nonce = _randomSource.GetRandomBytes(encryptor.ExpectedNonceSize);
            header.Compression = compression;

            try
            {
                if (string.Equals(CompressionGzip, header.Compression, StringComparison.InvariantCultureIgnoreCase))
                    message = CompressUtils.Compress(message);

                byte[] truncatedKey = CryptoUtils.TruncateKey(key, encryptor.ExpectedKeySize);
                byte[] cipher = encryptor.Encrypt(message, truncatedKey, header.Nonce);
                return CryptoHeaderPacker.PackHeaderAndCypher(header, cipher);
            }
            catch (Exception ex)
            {
                throw new CryptoException("An unexpected error occured, while encrypting the message.", ex);
            }
        }

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] packedCipher, SecureString password)
        {
            if (packedCipher == null)
                throw new ArgumentNullException("packedCipher");

            CryptoHeader header;
            byte[] cipher;
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedCipher, PackageName, out header, out cipher);

            ISymmetricEncryptionAlgorithm decryptor = new SymmetricEncryptionAlgorithmFactory().CreateAlgorithm(header.AlgorithmName);
            IKeyDerivationFunction kdf = new KeyDerivationFactory().CreateKdf(header.KdfName);

            try
            {
                byte[] key = kdf.DeriveKeyFromPassword(password, decryptor.ExpectedKeySize, header.Salt, header.Cost);
                byte[] message = decryptor.Decrypt(cipher, key, header.Nonce);

                if (string.Equals(CompressionGzip, header.Compression, StringComparison.InvariantCultureIgnoreCase))
                    message = CompressUtils.Decompress(message);

                return message;
            }
            catch (Exception ex)
            {
                throw new CryptoDecryptionException("Could not decrypt cipher, probably because the key is wrong.", ex);
            }
        }

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] packedCipher, byte[] key)
        {
            if (packedCipher == null)
                throw new ArgumentNullException("packedCipher");

            CryptoHeader header;
            byte[] cipher;
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedCipher, PackageName, out header, out cipher);

            ISymmetricEncryptionAlgorithm decryptor = new SymmetricEncryptionAlgorithmFactory().CreateAlgorithm(header.AlgorithmName);

            try
            {
                byte[] truncatedKey = CryptoUtils.TruncateKey(key, decryptor.ExpectedKeySize);
                byte[] message = decryptor.Decrypt(cipher, truncatedKey, header.Nonce);

                if (string.Equals(CompressionGzip, header.Compression, StringComparison.InvariantCultureIgnoreCase))
                    message = CompressUtils.Decompress(message);

                return message;
            }
            catch (Exception ex)
            {
                throw new CryptoDecryptionException("Could not decrypt cipher, probably because the key is wrong.", ex);
            }
        }

        private void ValidatePassword(SecureString password)
        {
            if ((password == null) || (password.Length < MinPasswordLength))
                throw new CryptoException(string.Format("The password must be at least {0} characters in length.", MinPasswordLength));
        }
    }
}
