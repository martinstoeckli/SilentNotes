// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml.Linq;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>. It tries to decrypt an
    /// already downloaded cloud repository with the known transfer codes.
    /// </summary>
    public class DecryptCloudRepositoryStep : SynchronizationStoryBoardStepBase
    {
        protected readonly ILanguageService _languageService;
        protected readonly IFeedbackService _feedbackService;
        protected readonly ISettingsService _settingsService;
        protected readonly INoteRepositoryUpdater _noteRepositoryUpdater;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptCloudRepositoryStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public DecryptCloudRepositoryStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            INoteRepositoryUpdater noteRepositoryUpdater)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _settingsService = settingsService;
            _noteRepositoryUpdater = noteRepositoryUpdater;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            try
            {
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                byte[] binaryCloudRepository = StoryBoard.LoadFromSession<byte[]>(SynchronizationStorySessionKey.BinaryCloudRepository);

                // Try to decode with all possible transfer codes
                bool successfullyDecryptedRepository = TryDecryptWithAllTransferCodes(
                    settings, binaryCloudRepository, out byte[] decryptedRepository);

                if (successfullyDecryptedRepository)
                {
                    // Deserialize and update repository
                    XDocument cloudRepositoryXml = XmlUtils.LoadFromXmlBytes(decryptedRepository);
                    if (_noteRepositoryUpdater.IsTooNewForThisApp(cloudRepositoryXml))
                        throw new SynchronizationStoryBoard.UnsuportedRepositoryRevisionException();

                    _noteRepositoryUpdater.Update(cloudRepositoryXml);
                    NoteRepositoryModel cloudRepository = XmlUtils.DeserializeFromXmlDocument<NoteRepositoryModel>(cloudRepositoryXml);

                    // Continue with next step
                    StoryBoard.StoreToSession(SynchronizationStorySessionKey.CloudRepository, cloudRepository);
                    await StoryBoard.ContinueWith(SynchronizationStoryStepId.IsSameRepository);
                }
                else
                {
                    bool existsUserEnteredTransferCode = StoryBoard.TryLoadFromSession<string>(SynchronizationStorySessionKey.UserEnteredTransferCode, out _);
                    if (existsUserEnteredTransferCode)
                    {
                        // Keep transfercode page open and show message
                        _feedbackService.ShowToast(_languageService["sync_error_transfercode"]);
                    }
                    else
                    {
                        // Open transfercode page
                        await StoryBoard.ContinueWith(SynchronizationStoryStepId.ShowTransferCode);
                    }
                }
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }

        /// <summary>
        /// Tries to decrypt the binary repository testing all known transfer codes.
        /// If the decryption is successful, the transfer code and the encryption mode is updated
        /// and saved.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <param name="binaryCloudRepository">The repository to decrypt.</param>
        /// <param name="decryptedRepository">The decrypted repository, or null if the decryption
        /// was not successful.</param>
        /// <returns>Returns true if the decryption was successful, otherwise false.</returns>
        protected bool TryDecryptWithAllTransferCodes(SettingsModel settings, byte[] binaryCloudRepository, out byte[] decryptedRepository)
        {
            bool result = false;
            decryptedRepository = null;

            EncryptorDecryptor encryptor = new EncryptorDecryptor("SilentNotes");
            List<string> transferCodesToTry = ListTransferCodesToTry(settings);
            int index = 0;
            while (!result && index < transferCodesToTry.Count)
            {
                string transferCodeCandidate = transferCodesToTry[index];
                result = TryDecryptRepositoryWithTransfercode(encryptor, binaryCloudRepository, transferCodeCandidate, out decryptedRepository);
                if (result)
                {
                    // Store transfercode and encryption mode if necessary
                    if (AdoptTransferCode(settings, transferCodeCandidate))
                    {
                        _settingsService.TrySaveSettingsToLocalDevice(settings);
                    }
                }
                index++;
            }
            return result;
        }

        /// <summary>
        /// Adopt the <paramref name="transferCode"/> to the settings, if it differs from the
        /// already stored one.
        /// </summary>
        /// <param name="settings">Settings to update.</param>
        /// <param name="transferCode">Transfer code to adopt.</param>
        /// <returns>Returns true if the transfer code was adopted, otherwise false.</returns>
        private static bool AdoptTransferCode(SettingsModel settings, string transferCode)
        {
            if (!string.Equals(settings.TransferCode, transferCode))
            {
                settings.TransferCode = transferCode;
                return true;
            }
            return false;
        }

        private List<string> ListTransferCodesToTry(SettingsModel settings)
        {
            var result = new List<string>();
            bool existsUserEnteredTransferCode = StoryBoard.TryLoadFromSession<string>(SynchronizationStorySessionKey.UserEnteredTransferCode, out string userEnteredTransferCode);
            if (existsUserEnteredTransferCode)
            {
                result.Add(userEnteredTransferCode);
            }
            else
            {
                if (!string.IsNullOrEmpty(settings.TransferCode))
                    result.Add(settings.TransferCode);
                result.AddRange(settings.TransferCodeHistory);
            }
            return result;
        }

        private bool TryDecryptRepositoryWithTransfercode(EncryptorDecryptor encryptor, byte[] binaryCloudRepository, string transferCode, out byte[] decryptedRepository)
        {
            try
            {
                decryptedRepository = encryptor.Decrypt(binaryCloudRepository, CryptoUtils.StringToSecureString(transferCode));
                return true;
            }
            catch (CryptoExceptionInvalidCipherFormat)
            {
                // If the downloaded repository is invalid, this is serious and we should not continue
                throw;
            }
            catch (CryptoUnsupportedRevisionException)
            {
                throw;
            }
            catch (Exception)
            {
                // Could not decrypt with this transfercode
                decryptedRepository = null;
                return false;
            }
        }
    }
}
