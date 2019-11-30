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
        /// <summary>The highest revision of the header which can be handled by this application.</summary>
        public const int NewestSupportedRevision = 2;

        /// <summary>
        /// Gets or sets the package name, <see cref="ICryptor.PackageName"/>.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the revision of the header. This can be used to detect headers which are
        /// created by future app versions.
        /// </summary>
        public int Revision { get; set; }

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

        /// <summary>Gets or sets the used compression algorithm if any, otherwise null.</summary>
        public string Compression { get; set; }
    }
}
