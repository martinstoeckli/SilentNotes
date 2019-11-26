// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// Extends the <see cref="StoryBoardStepBase"/> class by functions specific to a
    /// <see cref="SynchronizationStoryBoard"/>.
    /// </summary>
    public abstract class SynchronizationStoryBoardStepBase : StoryBoardStepBase
    {
        /// <inheritdoc/>
        protected SynchronizationStoryBoardStepBase(Enum stepId, IStoryBoard storyBoard)
            : base(stepId, storyBoard)
        {
        }

        /// <summary>
        /// Checks the type of the exception and shows an appropriate error message.
        /// </summary>
        /// <param name="ex">Exception to handle.</param>
        /// <param name="feedbackService">Dialog service to show the error message.</param>
        /// <param name="languageService">The language service.</param>
        public static void ShowExceptionMessage(Exception ex, IFeedbackService feedbackService, ILanguageService languageService)
        {
            if (ex is ConnectionFailedException)
            {
                feedbackService.ShowToast(languageService["sync_error_connection"]);
            }
            else if (ex is AccessDeniedException)
            {
                feedbackService.ShowToast(languageService["sync_error_privileges"]);
            }
            else if (ex is CryptoExceptionInvalidCipherFormat)
            {
                feedbackService.ShowToast(languageService["sync_error_repository"]);
            }
            else if (ex is CryptoUnsupportedRevisionException)
            {
                feedbackService.ShowToast(languageService["sync_error_revision"]);
            }
            else if (ex is SynchronizationStoryBoard.UnsuportedRepositoryRevisionException)
            {
                feedbackService.ShowToast(languageService["sync_error_revision"]);
            }
            else
            {
                feedbackService.ShowToast(languageService["sync_error_generic"]);
            }
        }

        internal static byte[] EncryptRepository(NoteRepositoryModel repository, string transferCode, ICryptoRandomService randomService, string encryptionAlgorithm)
        {
            byte[] binaryRepository = XmlUtils.SerializeToXmlBytes(repository);
            EncryptorDecryptor encryptor = new EncryptorDecryptor("SilentNotes");

            // The key derivation cost is set to low, because we can be sure that the transferCode
            // is a very strong password, and to not overload slow mobile devices.
            return encryptor.Encrypt(
                binaryRepository, transferCode,
                KeyDerivationCostType.Low,
                randomService,
                encryptionAlgorithm,
                Pbkdf2.CryptoKdfName,
                EncryptorDecryptor.CompressionGzip);
        }
    }
}
