// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// A data protection service uses OS functionality to encrypt/decrypt data, whithout needing
    /// hard-coded keys or user passwords. The OS usually keeps the keys private and makes sure
    /// they cannot be extracted by any application. Not even the owning application should be able
    /// to extract the key, but it can us it to encrypt/decrypt data, e.g. passwords or master keys.
    /// </summary>
    public interface IDataProtectionService
    {
        /// <summary>
        /// Encrypts the data, which e.g. can be a password or a master key.
        /// </summary>
        /// <param name="unprotectedData">The data to protect.</param>
        /// <returns>Encrypted and base64 encoded data.</returns>
        string Protect(byte[] unprotectedData);

        /// <summary>
        /// Decrypts the data, formerly protected with <see cref="Protect(byte[])"/>.
        /// </summary>
        /// <param name="protectedData">The encrypted data.</param>
        /// <returns>The plain text data.</returns>
        byte[] Unprotect(string protectedData);
    }
}
