// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// Base class for all story steps of the synchronization story.
    /// </summary>
    internal abstract class SynchronizationStoryStepBase : StoryStepBase<SynchronizationStoryModel>
    {
        /// <inheritdoc/>
        protected override string TranslateException(Exception ex, IServiceProvider serviceProvider)
        {
            ILanguageService languageService = serviceProvider.GetService<ILanguageService>();

            if (ex is ConnectionFailedException)
            {
                return languageService["sync_error_connection"];
            }
            else if (ex is AccessDeniedException)
            {
                return languageService["sync_error_privileges"];
            }
            else if (ex is CryptoExceptionInvalidCipherFormat)
            {
                return languageService["sync_error_repository"];
            }
            else if (ex is CryptoUnsupportedRevisionException)
            {
                return languageService["sync_error_revision"];
            }
            else if (ex is SynchronizationStoryExceptions.UnsuportedRepositoryRevisionException)
            {
                return languageService["sync_error_revision"];
            }
            else
            {
                return languageService["sync_error_generic"];
            }
        }

        internal static byte[] EncryptRepository(NoteRepositoryModel repository, string transferCode, ICryptoRandomService randomService, string encryptionAlgorithm)
        {
            byte[] binaryRepository = XmlUtils.SerializeToXmlBytes(repository);
            ICryptor encryptor = new Cryptor("SilentNotes", randomService);

            // The key derivation cost is set to low, because we can be sure that the transferCode
            // is a very strong password, and to not overload slow mobile devices.
            return encryptor.Encrypt(
                binaryRepository,
                CryptoUtils.StringToSecureString(transferCode),
                KeyDerivationCostType.Low,
                encryptionAlgorithm,
                Pbkdf2.CryptoKdfName,
                Cryptor.CompressionGzip);
        }
    }
}
