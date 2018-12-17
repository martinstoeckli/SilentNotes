// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Crypto.KeyDerivation
{
    /// <summary>
    /// Implementations of this interface are key derivation functions, to get an encryption-key
    /// from a given user password.
    /// </summary>
    public interface IKeyDerivationFunction
    {
        /// <summary>
        /// Gets the name of the key derivation function.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Generates a key from a password, to be used for encryption or decryption.
        /// </summary>
        /// <param name="password">User password.</param>
        /// <param name="expectedKeySizeBytes">Size of the key, expected by the encryption
        /// algorithm.</param>
        /// <param name="salt">The salt to use.</param>
        /// <param name="cost">The cost factor which controls the necessary time for the
        /// calculation (the longer the more safe from brute-forcing).</param>
        /// <returns>Key for decryption.</returns>
        byte[] DeriveKeyFromPassword(string password, int expectedKeySizeBytes, byte[] salt, int cost);

        /// <summary>
        /// Gets the size of the salt in bytes, which is expected by the implementing key
        /// derivation function.
        /// </summary>
        int ExpectedSaltSizeBytes { get; }

        /// <summary>
        /// Gets the recommended cost factor, the implementing algorithm decides about the meaning
        /// of this value.
        /// </summary>
        /// <param name="costType">The cost type.</param>
        /// <returns>The recommended cost factor.</returns>
        int RecommendedCost(KeyDerivationCostType costType);
    }
}
