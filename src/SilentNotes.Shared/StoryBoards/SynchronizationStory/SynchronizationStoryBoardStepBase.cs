// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// Extends the <see cref="StoryBoardStepBase"/> class by functions specific to a
    /// <see cref="SynchronizationStoryBoard"/>.
    /// </summary>
    public abstract class SynchronizationStoryBoardStepBase : StoryBoardStepBase
    {
        /// <inheritdoc/>
        public SynchronizationStoryBoardStepBase(int stepId, IStoryBoard storyBoard)
            : base(stepId, storyBoard)
        {
        }

        /// <summary>
        /// Gets a value indicating, whether the story runs silently in the background. If this
        /// value is true, no GUI should be involved and missing information should stop the story.
        /// The SilentMode is used for auto-sync at application startup/shutdown.
        /// </summary>
        public bool IsRunningInSilentMode
        {
            get { return StoryBoard.SilentMode; }
        }

        /// <summary>
        /// Checks the type of the exception and shows an appropriate error message.
        /// </summary>
        /// <param name="ex">Exception to handle.</param>
        /// <param name="feedbackService">Dialog service to show the error message.</param>
        /// <param name="languageService">The language service.</param>
        public void ShowExceptionMessage(Exception ex, IFeedbackService feedbackService, ILanguageService languageService)
        {
            if (IsRunningInSilentMode)
                return;

            if (ex is CloudStorageConnectionException)
            {
                feedbackService.ShowToast(languageService["sync_error_connection"]);
            }
            else if (ex is CloudStorageForbiddenException)
            {
                feedbackService.ShowToast(languageService["sync_error_privileges"]);
            }
            else if (ex is CryptoExceptionInvalidCipherFormat)
            {
                feedbackService.ShowToast(languageService["sync_error_repository"]);
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
            return encryptor.Encrypt(binaryRepository, transferCode, Crypto.KeyDerivation.KeyDerivationCostType.Low, randomService, encryptionAlgorithm);
        }
    }
}
