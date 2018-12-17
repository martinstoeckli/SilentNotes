// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Holds the parameters used for encryption and decryption. This header can be packed
    /// together with the cipher by the <see cref="CryptoHeaderPacker"/>.
    /// </summary>
    public class CryptoHeader
    {
        /// <summary>
        /// Gets or sets the application name.
        /// This value is used to recognize owned ciphers.
        /// </summary>
        public string AppName { get; set; }

        /// <summary>Gets or sets the used encryption algorithm.</summary>
        public string AlgorithmName { get; set; }

        /// <summary>Gets or sets the used nonce for encryption.</summary>
        public byte[] Nonce { get; set; }

        /// <summary>Gets or sets the used key derivation function to build the key.</summary>
        public string KdfName { get; set; }

        /// <summary>Gets or sets the used salt to build the key.</summary>
        public byte[] Salt { get; set; }

        /// <summary>Gets or sets the used cost factor for the key derivation function.</summary>
        public string Cost { get; set; }
    }
}
