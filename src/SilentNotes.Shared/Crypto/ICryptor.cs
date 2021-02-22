// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Security;
using SilentNotes.Crypto.KeyDerivation;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Use the cryptor to encrypt or decrypt data. It handles the encryption as well as the packing
    /// of the necessary information to decrypt, like the algorithm and more.
    /// <example><code>
    /// ICryptor cryptor = new Cryptor("MyAppName");
    /// </code></example>
    /// </summary>
    public interface ICryptor
    {
        /// <summary>
        /// Gets a name describing the content of the encrypted data. This way an application can
        /// distinguish its own encrypted data from other encrypted files.
        /// </summary>
        string PackageName { get; }

        /// <summary>
        /// Encrypts a message with a user password, and adds a header containing all information
        /// necessary for the decryption (algorithm, nonce, salt, ...).
        /// </summary>
        /// <param name="message">Plain text message to encrypt.</param>
        /// <param name="password">Password to use for encryption, minimum length is 5 characters,
        /// recommended are at least 8 characters.</param>
        /// <param name="costType">The cost type to use for encryption.</param>
        /// <param name="encryptorName">The name of an encryption algorithm which shall be used to
        /// do the encryption.</param>
        /// <param name="kdfName">The name of a key derivation function, which can convert the
        /// password to a key.</param>
        /// <param name="compression">The name of the compression algorithm, or null if no compression
        /// should happen before the encryption. Currently supported is <see cref="Cryptor.CompressionGzip"/>.</param>
        /// <returns>A binary array containing the cipher.</returns>
        byte[] Encrypt(
            byte[] message,
            SecureString password,
            KeyDerivationCostType costType,
            string encryptorName,
            string kdfName = Pbkdf2.CryptoKdfName,
            string compression = null);

        /// <summary>
        /// Encrypts a message with a user password, and adds a header containing all information
        /// necessary for the decryption (algorithm, nonce, salt, ...).
        /// </summary>
        /// <param name="message">Plain text message to encrypt.</param>
        /// <param name="key">Key to use for encryption. The length of the key must be equal or longer
        /// than <see cref="SymmetricEncryption.ISymmetricEncryptionAlgorithm.ExpectedKeySize"/>.</param>
        /// <param name="encryptorName">The name of an encryption algorithm which shall be used to
        /// do the encryption.</param>
        /// <param name="compression">The name of the compression algorithm, or null if no compression
        /// should happen before the encryption. Currently supported is <see cref="Cryptor.CompressionGzip"/>.</param>
        /// <returns>A binary array containing the cipher.</returns>
        byte[] Encrypt(
            byte[] message,
            byte[] key,
            string encryptorName,
            string compression = null);

        /// <summary>
        /// Decrypts a cipher, which was encrypted with <see cref="Encrypt(byte[], SecureString, KeyDerivationCostType, string, string, string)"/>.
        /// </summary>
        /// <param name="packedCipher">The cipher containing a header with the necessary parameters
        /// for decryption.</param>
        /// <param name="password">The password which was used for encryption.</param>
        /// <exception cref="CryptoExceptionInvalidCipherFormat">Thrown if it doesn't contain a valid header.</exception>
        /// <exception cref="CryptoUnsupportedRevisionException">Thrown if it was packed with a future incompatible version.</exception>
        /// <exception cref="CryptoDecryptionException">Thrown if there was an error decrypting the cipher.</exception>
        /// <returns>The plain text message.</returns>
        byte[] Decrypt(byte[] packedCipher, SecureString password);

        /// <summary>
        /// Decrypts a cipher, which was encrypted with <see cref="Encrypt(byte[], byte[], string, string)"/>.
        /// </summary>
        /// <param name="packedCipher">The cipher containing a header with the necessary parameters
        /// for decryption.</param>
        /// <param name="key">The key which was used for encryption.</param>
        /// <exception cref="CryptoExceptionInvalidCipherFormat">Thrown if it doesn't contain a valid header.</exception>
        /// <exception cref="CryptoUnsupportedRevisionException">Thrown if it was packed with a future incompatible version.</exception>
        /// <exception cref="CryptoDecryptionException">Thrown if there was an error decrypting the cipher.</exception>
        /// <returns>The plain text message.</returns>
        byte[] Decrypt(byte[] packedCipher, byte[] key);
    }
}
