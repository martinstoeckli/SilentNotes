// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Services;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IDataProtectionService"/> interface for the UWP platform.
    /// Credits to:
    ///   https://msicc.net/using-the-built-in-uwp-data-protection-for-data-encryption/
    /// </summary>
    internal class DataProtectionService : IDataProtectionService
    {
        private const string ProtectionScope = "LOCAL = machine";

        /// <inheritdoc/>
        public string Protect(byte[] unprotectedData)
        {
            return Task.Run(async () => await ProtectAsync(unprotectedData)).Result;
        }

        /// <inheritdoc/>
        public byte[] Unprotect(string protectedData)
        {
            return Task.Run(async () => await UnprotectDataAsync(protectedData)).Result;
        }

        private async Task<string> ProtectAsync(byte[] unprotectedData)
        {
            DataProtectionProvider protectionProvider = new DataProtectionProvider(ProtectionScope);
            IBuffer unprotectedBuffer = CryptographicBuffer.CreateFromByteArray(unprotectedData);
            IBuffer protectedBuffer = await protectionProvider.ProtectAsync(unprotectedBuffer);
            return CryptographicBuffer.EncodeToBase64String(protectedBuffer);
        }

        private async Task<byte[]> UnprotectDataAsync(string protectedData)
        {
            DataProtectionProvider protectionProvider = new DataProtectionProvider(ProtectionScope);
            IBuffer protectedBuffer = CryptographicBuffer.DecodeFromBase64String(protectedData);
            IBuffer unprotectedBuffer = await protectionProvider.UnprotectAsync(protectedBuffer);
            CryptographicBuffer.CopyToByteArray(unprotectedBuffer, out byte[] result);
            return result;
        }
    }
}
