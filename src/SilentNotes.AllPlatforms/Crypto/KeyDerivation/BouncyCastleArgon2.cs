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
    public class BouncyCastleArgon2 : IKeyDerivationFunction
    {
        /// <summary>The name of the XChaCha20-Poly1305 encryption algorithm.</summary>
        public const string CryptoAlgorithmName = "argon2id";

        private const int SaltSizeBytes = 16; // 128 bits

        public string Name => CryptoAlgorithmName;

        public byte[] DeriveKeyFromPassword(SecureString password, int expectedKeySizeBytes, byte[] salt, int cost)
        {
            if ((password == null) || (password.Length == 0))
                throw new CryptoException("The password cannot be empty.");
            if (cost < 1)
                throw new CryptoException("The cost factor is too small.");

            byte[] binaryPassword = SecureStringExtensions.SecureStringToBytes(password, Encoding.UTF8);
            try
            {
                // See also: https://github.com/jedisct1/libsodium/blob/master/src/libsodium/include/sodium/crypto_pwhash_argon2id.h
                Argon2Parameters argonParameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                    .WithSalt(salt)
                    .WithParallelism(1)
                    .WithMemoryAsKB(64000)
                    //.WithMemoryPowOfTwo(16) // 2^26 = 64 Mib = libsodium crypto_pwhash_argon2id_MEMLIMIT_INTERACTIVE
                    .WithIterations(cost)
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

        public int RecommendedCost(KeyDerivationCostType costType)
        {
            switch (costType)
            {
                case KeyDerivationCostType.Low:
                    return 1;
                case KeyDerivationCostType.High:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException("costType");
            }
        }
    }
}
