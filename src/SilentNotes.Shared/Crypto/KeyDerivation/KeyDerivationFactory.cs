// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Crypto.KeyDerivation
{
    /// <summary>
    /// Factory to create a key derivation function.
    /// </summary>
    public class KeyDerivationFactory
    {
        /// <summary>
        /// Creates the correct implementation of the key-derivation-function from its name.
        /// </summary>
        /// <param name="kdfName">Name of the required key derivation function.</param>
        /// <returns>Instance of the given key derivation function.</returns>
        public IKeyDerivationFunction CreateKdf(string kdfName)
        {
            switch (kdfName)
            {
                // Add other algorithms if necessary
                case Pbkdf2.CryptoKdfName:
                    return new Pbkdf2();
                default:
                    throw new CryptoException(string.Format("Unknown key derivation function '{0}'", kdfName));
            }
        }
    }
}
