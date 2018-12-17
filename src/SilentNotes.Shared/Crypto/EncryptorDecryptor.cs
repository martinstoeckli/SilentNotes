// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// This is the starting point for the crypto module. Use this class to encrypt and decrypt
    /// messages.
    /// </summary>
    public class EncryptorDecryptor
    {
        private const int MinPasswordLength = 7;
        private readonly string _appName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptorDecryptor"/> class.
        /// </summary>
        /// <param name="appName">This name will be added to the header,
        /// so an application can recognize its own encrypted data.</param>
        public EncryptorDecryptor(string appName)
        {
            _appName = appName;
        }

        /// <summary>
        /// Encrypts a message with a user password, and adds a header containing all information
        /// necessary for the decryption (algorithm, nonce, salt, ...).
        /// </summary>
        /// <param name="message">Plain text message to encrypt.</param>
        /// <param name="password">Password to use for encryption, minimum length is 7 characters.</param>
        /// <param name="costType">The cost type to use for encryption.</param>
        /// <param name="randomSource">A cryptographically safe random source.</param>
        /// <param name="encryptorName">The name of an encryption algorithm which shall be used to
        /// do the encryption.</param>
        /// <param name="kdfName">The name of a key derivation function, which can convert the
        /// password to a key.</param>
        /// <returns>A binary array containing the cipher.</returns>
        public byte[] Encrypt(
            byte[] message,
            string password,
            KeyDerivationCostType costType, 
            ICryptoRandomSource randomSource,
            string encryptorName,
            string kdfName = Pbkdf2.CryptoKdfName)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            ValidatePassword(password);
            if (randomSource == null)
                throw new ArgumentNullException("randomSource");
            if (string.IsNullOrWhiteSpace(encryptorName))
                encryptorName = BouncyCastleAesGcm.CryptoAlgorithmName;
            if (string.IsNullOrWhiteSpace(kdfName))
                encryptorName = Pbkdf2.CryptoKdfName;
            ISymmetricEncryptionAlgorithm encryptor = new SymmetricEncryptionAlgorithmFactory().CreateAlgorithm(encryptorName);
            IKeyDerivationFunction kdf = new KeyDerivationFactory().CreateKdf(kdfName);

            // Prepare header
            CryptoHeader header = new CryptoHeader();
            header.AppName = _appName;
            header.AlgorithmName = encryptor.Name;
            header.Nonce = randomSource.GetRandomBytes(encryptor.ExpectedNonceSize);
            header.KdfName = kdf.Name;
            header.Salt = randomSource.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            int cost = kdf.RecommendedCost(costType);
            header.Cost = cost.ToString();

            try
            {
                byte[] key = kdf.DeriveKeyFromPassword(password, encryptor.ExpectedKeySize, header.Salt, cost);
                byte[] cipher = encryptor.Encrypt(message, key, header.Nonce);
                return CryptoHeaderPacker.PackHeaderAndCypher(header, cipher);
            }
            catch (Exception ex)
            {
                throw new CryptoException("An unexpected error occured, while encrypting the message.", ex);
            }
        }

        /// <summary>
        /// Decrypts a cipher, which was encrypted with <see cref="Encrypt(byte[],string,KeyDerivationCostType,ICryptoRandomSource,string,string)"/>.
        /// </summary>
        /// <param name="packedCipher">The cipher containing a header with the
        /// necessary parameters for decryption.</param>
        /// <param name="password">The password which was used for encryption.</param>
        /// <returns>The plain text message.</returns>
        public byte[] Decrypt(byte[] packedCipher, string password)
        {
            if (packedCipher == null)
                throw new ArgumentNullException("packedCipher");

            CryptoHeader header;
            byte[] cipher;
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedCipher, out header, out cipher);
            if (_appName != header.AppName)
                throw new CryptoExceptionInvalidCipherFormat();

            ISymmetricEncryptionAlgorithm decryptor = new SymmetricEncryptionAlgorithmFactory().CreateAlgorithm(header.AlgorithmName);
            IKeyDerivationFunction kdf = new KeyDerivationFactory().CreateKdf(header.KdfName);

            try
            {
                int cost = int.Parse(header.Cost);
                byte[] key = kdf.DeriveKeyFromPassword(password, decryptor.ExpectedKeySize, header.Salt, cost);
                byte[] message = decryptor.Decrypt(cipher, key, header.Nonce);
                return message;
            }
            catch (Exception ex)
            {
                throw new CryptoDecryptionException("Could not decrypt cipher, probably because the key is wrong.", ex);
            }
        }

        /// <summary>
        /// Extracts the name of the used algorithm from the cipher.
        /// </summary>
        /// <param name="packedCipher">Cipher with header.</param>
        /// <returns>The name of the used algorithm.</returns>
        public string ExtractAlgorithmName(byte[] packedCipher)
        {
            if (packedCipher == null)
                throw new ArgumentNullException("packedCipher");

            CryptoHeaderPacker.UnpackHeaderAndCipher(packedCipher, out CryptoHeader header, out byte[] cipher);
            if (_appName != header.AppName)
                throw new CryptoExceptionInvalidCipherFormat();

             return header.AlgorithmName;
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || (password.Length < MinPasswordLength))
                throw new CryptoException(string.Format("The password must be at least {0} characters in length.", MinPasswordLength));
        }
    }
}
