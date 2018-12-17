// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Crypto.KeyDerivation;

namespace SilentNotes.Crypto.SymmetricEncryption
{
    /// <summary>
    /// Implementations of this interface can encrypt data with a specific encryption algorihtm.
    /// </summary>
    public interface ISymmetricEncryptionAlgorithm
    {
        /// <summary>
        /// Gets the name of the encryptor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Encrypts a message with a binary key. The length of the expected key can be determined
        /// by <see cref="ExpectedKeySize"/>. To get a key from a user password, use an
        /// implementation of the <see cref="IKeyDerivationFunction"/> interface first.
        /// </summary>
        /// <param name="message">Plaintext message to encrypt.</param>
        /// <param name="key">The key to use for encryption.</param>
        /// <param name="nonce">The nonce required for encryption.</param>
        /// <returns>Encrypted cipher</returns>
        byte[] Encrypt(byte[] message, byte[] key, byte[] nonce);

        /// <summary>
        /// Decrypts the cipher with a binary key.
        /// </summary>
        /// <param name="cipher">The cipher to decrypt.</param>
        /// <param name="key">The key to use for decryption.</param>
        /// <param name="nonce">The nonce which was used for encryption.</param>
        /// <returns>Decrypted message.</returns>
        byte[] Decrypt(byte[] cipher, byte[] key, byte[] nonce);

        /// <summary>
        /// Gets the size of the key in bytes, which is expected by the implementing encryption
        /// algorithm.
        /// </summary>
        /// <returns>Key size in bytes.</returns>
        int ExpectedKeySize { get; }

        /// <summary>
        /// Gets the size of the nonce in bytes, which is expected by the implementing encryption
        /// algorithm.
        /// </summary>
        int ExpectedNonceSize { get; }
    }
}
