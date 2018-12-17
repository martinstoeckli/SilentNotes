// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Security.Cryptography;

namespace SilentNotes.Crypto.KeyDerivation
{
    /// <summary>
    /// Implementation of the <see cref="IKeyDerivationFunction"/> interface, implementing the
    /// PBKDF2 algorithm with SHA1 and HMAC.
    /// </summary>
    public class Pbkdf2 : IKeyDerivationFunction
    {
        /// <summary>The name of the PBKDF2 key derivation function.</summary>
        public const string CryptoKdfName = "pbkdf2";

        private const int SaltSizeBytes = 16; // 128 bits

        /// <inheritdoc />
        public string Name
        {
            get { return CryptoKdfName; }
        }

        /// <inheritdoc />
        public byte[] DeriveKeyFromPassword(string password, int expectedKeySizeBytes, byte[] salt, int cost)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new CryptoException("The password cannot be empty.");
            if (cost < 1)
                throw new CryptoException("The cost factor is too small.");

            byte[] binaryPassword = CryptoUtils.StringToBytes(password);
            Rfc2898DeriveBytes kdf = new Rfc2898DeriveBytes(binaryPassword, salt, cost);
            return kdf.GetBytes(expectedKeySizeBytes);
        }

        /// <inheritdoc />
        public int ExpectedSaltSizeBytes
        {
            get { return SaltSizeBytes; }
        }

        /// <inheritdoc />
        public int RecommendedCost(KeyDerivationCostType costType)
        {
            switch (costType)
            {
                case KeyDerivationCostType.Low:
                    return 1000;
                case KeyDerivationCostType.High:
                    return 10000;
                default:
                    throw new ArgumentOutOfRangeException("costType");
            }
        }
    }
}
