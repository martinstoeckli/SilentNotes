// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Security;
using System.Text;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using VanillaCloudStorageClient;

namespace SilentNotes.Crypto.KeyDerivation
{
    /// <summary>
    /// Implementation of the <see cref="IKeyDerivationFunction"/> interface, implementing the
    /// Argon2 algorithm.
    /// </summary>
    public class BouncyCastleArgon2 : IKeyDerivationFunction
    {
        /// <summary>The name of the Argon2 key derivation function.</summary>
        public const string CryptoKdfName = "argon2id";

        private const int SaltSizeBytes = 16; // 128 bits

        /// <inheritdoc />
        public string Name => CryptoKdfName;

        /// <inheritdoc />
        public byte[] DeriveKeyFromPassword(SecureString password, int expectedKeySizeBytes, byte[] salt, string cost)
        {
            if ((password == null) || (password.Length == 0))
                throw new CryptoException("The password cannot be empty.");
            if (!Argon2Cost.TryParse(cost, out Argon2Cost costParameters))
                throw new CryptoException("Invalid cost parameter.");

            if (costParameters.MemoryKib < 1)
                throw new CryptoException("The memory cost factor is too small.");
            if (costParameters.Iterations < 1)
                throw new CryptoException("The iteration cost factor is too small.");
            if (costParameters.Parallelism < 1)
                throw new CryptoException("The parallelism cost factor is too small.");

            byte[] binaryPassword = SecureStringExtensions.SecureStringToBytes(password, Encoding.UTF8);
            try
            {
                // See also: https://github.com/jedisct1/libsodium/blob/master/src/libsodium/include/sodium/crypto_pwhash_argon2id.h
                Argon2Parameters argonParameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                    .WithSalt(salt)
                    .WithMemoryAsKB(costParameters.MemoryKib)
                    .WithIterations(costParameters.Iterations)
                    .WithParallelism(costParameters.Parallelism)
                    .Build();

                Argon2BytesGenerator argonGenerator = new Argon2BytesGenerator();
                argonGenerator.Init(argonParameters);

                byte[] result = new byte[expectedKeySizeBytes];
                argonGenerator.GenerateBytes(binaryPassword, result, 0, result.Length);
                return result;
            }
            finally
            {
                CryptoUtils.CleanArray(binaryPassword);
            }
        }

        public int ExpectedSaltSizeBytes => SaltSizeBytes;

        public string RecommendedCost(KeyDerivationCostType costType)
        {
            Argon2Cost result;
            switch (costType)
            {
                case KeyDerivationCostType.Low:
                    result = new Argon2Cost
                    {
                        MemoryKib = 1024,
                        Iterations = 2,
                        Parallelism = 1,
                    };
                    break;
                case KeyDerivationCostType.High:
                    result = new Argon2Cost
                    {
                        MemoryKib = 64000,
                        Iterations = 3,
                        Parallelism = 1,
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(costType));
            }
            return result.Format();
        }
    }
}
