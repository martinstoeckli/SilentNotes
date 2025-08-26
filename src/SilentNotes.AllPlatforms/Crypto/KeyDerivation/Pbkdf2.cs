// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Globalization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using VanillaCloudStorageClient;

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
        public byte[] DeriveKeyFromPassword(SecureString password, int expectedKeySizeBytes, byte[] salt, string cost)
        {
            if ((password == null) || (password.Length == 0))
                throw new CryptoException("The password cannot be empty.");
            if (!int.TryParse(cost, NumberStyles.None, CultureInfo.InvariantCulture, out int iterations))
                throw new CryptoException("The cost parameter has an invalid format.");
            if (iterations < 1)
                throw new CryptoException("The cost factor is too small.");

            byte[] binaryPassword = SecureStringExtensions.SecureStringToBytes(password, Encoding.UTF8);
            try
            {
                Rfc2898DeriveBytes kdf = new Rfc2898DeriveBytes(binaryPassword, salt, iterations, HashAlgorithmName.SHA1);
                byte[] result = kdf.GetBytes(expectedKeySizeBytes);
                return result;
            }
            finally
            {
                CryptoUtils.CleanArray(binaryPassword);
            }
        }

        /// <inheritdoc />
        public int ExpectedSaltSizeBytes
        {
            get { return SaltSizeBytes; }
        }

        /// <inheritdoc />
        public string RecommendedCost(KeyDerivationCostType costType)
        {
            switch (costType)
            {
                case KeyDerivationCostType.Low:
                    return "1500";
                case KeyDerivationCostType.High:
                    return "20000"; // measured 600ms on a mid-range mobile device in 2023
                default:
                    throw new ArgumentOutOfRangeException(nameof(costType));
            }
        }
    }
}
