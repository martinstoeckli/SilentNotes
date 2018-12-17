// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Crypto.SymmetricEncryption
{
    /// <summary>
    /// Factory to create a symmetric encryption algorithm.
    /// </summary>
    public class SymmetricEncryptionAlgorithmFactory
    {
        /// <summary>
        /// Factory to create the correct implementation of the encryption algorithm, from its name.
        /// </summary>
        /// <param name="algorithmName">Name of the encryptor.</param>
        /// <returns>Instance of the given encryptor.</returns>
        public ISymmetricEncryptionAlgorithm CreateAlgorithm(string algorithmName)
        {
            // Add other algorithms if necessary
            switch (algorithmName)
            {
                case BouncyCastleAesGcm.CryptoAlgorithmName:
                    return new BouncyCastleAesGcm();
                case BouncyCastleTwofishGcm.CryptoAlgorithmName:
                    return new BouncyCastleTwofishGcm();
                default:
                    throw new CryptoException(string.Format("Unknown encryption algorithm '{0}'", algorithmName));
            }
        }
    }
}
