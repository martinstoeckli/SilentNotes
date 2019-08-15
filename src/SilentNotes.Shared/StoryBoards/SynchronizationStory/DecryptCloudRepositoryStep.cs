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
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly INoteRepositoryUpdater _noteRepositoryUpdater;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptCloudRepositoryStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public DecryptCloudRepositoryStep(
            int stepId,
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
                byte[] binaryCloudRepository = StoryBoard.LoadFromSession<byte[]>(SynchronizationStorySessionKey.BinaryCloudRepository.ToInt());
                List<string> transferCodesToTry = ListTransferCodesToTry(settings);

                // Try to decode with all possible transfer codes
                EncryptorDecryptor encryptor = new EncryptorDecryptor("SilentNotes");
                byte[] decryptedRepository = null;
                bool successfullyDecryptedRepository = false;
                int index = 0;
                while (!successfullyDecryptedRepository && index < transferCodesToTry.Count)
                {
                    string transferCodeCandidate = transferCodesToTry[index];
                    successfullyDecryptedRepository = TryDecryptRepositoryWithTransfercode(encryptor, binaryCloudRepository, transferCodeCandidate, out decryptedRepository);
                    if (successfullyDecryptedRepository)
                    {
                        // Store transfercode and encryption mode if necessary
                        if (AdoptTransferCode(settings, transferCodeCandidate) ||
                            AdoptEncryptionMode(settings, encryptor, binaryCloudRepository))
                        {
                            _settingsService.TrySaveSettingsToLocalDevice(settings);
                        }
                    }
                    index++;
                }

                if (successfullyDecryptedRepository)
                {
                    // Deserialize and update repository
                    XDocument cloudRepositoryXml = XmlUtils.LoadFromXmlBytes(decryptedRepository);
                    if (_noteRepositoryUpdater.IsTooNewForThisApp(cloudRepositoryXml))
                        throw new SynchronizationStoryBoard.UnsuportedRepositoryRevisionException();

                    _noteRepositoryUpdater.Update(cloudRepositoryXml);
                    NoteRepositoryModel cloudRepository = XmlUtils.DeserializeFromXmlDocument<NoteRepositoryModel>(cloudRepositoryXml);

                    // Continue with next step
                    StoryBoard.StoreToSession(SynchronizationStorySessionKey.CloudRepository.ToInt(), cloudRepository);
                    await StoryBoard.ContinueWith(SynchronizationStoryStepId.IsSameRepository.ToInt());
                }
                else
                {
                    bool existsUserEnteredTransferCode = StoryBoard.TryLoadFromSession<string>(SynchronizationStorySessionKey.UserEnteredTransferCode.ToInt(), out _);
                    if (existsUserEnteredTransferCode)
                    {
                        // Keep transfercode page open and show message
                        _feedbackService.ShowToast(_languageService["sync_error_transfercode"]);
                    }
                    else
                    {
                        // Open transfercode page
                        await StoryBoard.ContinueWith(SynchronizationStoryStepId.ShowTransferCode.ToInt());
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

        /// <summary>
        /// Adopts the encryption mode to the settings, if the settings are set accordingly and the
        /// mode differs from the stored one.
        /// </summary>
        /// <param name="settings">Settings to update.</param>
        /// <param name="encryptor">The eccryptor.</param>
        /// <param name="binaryCloudRepository">The repository containing the encryption mode.</param>
        /// <returns>Returns true if the encryption mode was adopted, otherwise false.</returns>
        private static bool AdoptEncryptionMode(SettingsModel settings, EncryptorDecryptor encryptor, byte[] binaryCloudRepository)
        {
            if (settings.AdoptCloudEncryptionAlgorithm)
            {
                string cloudEncryptionAlgorithm = encryptor.ExtractAlgorithmName(binaryCloudRepository);
                if (!string.Equals(settings.SelectedEncryptionAlgorithm, cloudEncryptionAlgorithm, StringComparison.OrdinalIgnoreCase))
                {
                    settings.SelectedEncryptionAlgorithm = cloudEncryptionAlgorithm;
                    return true;
                }
            }
            return false;
        }

        private List<string> ListTransferCodesToTry(SettingsModel settings)
        {
            var result = new List<string>();
            bool existsUserEnteredTransferCode = StoryBoard.TryLoadFromSession<string>(SynchronizationStorySessionKey.UserEnteredTransferCode.ToInt(), out string userEnteredTransferCode);
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
                decryptedRepository = encryptor.Decrypt(binaryCloudRepository, transferCode);
                return true;
            }
            catch (CryptoExceptionInvalidCipherFormat)
            {
                // If the downloaded repository is invalid, this is serioud and we should not continue
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
