// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It tries to decrypt an
    /// already downloaded cloud repository with the known transfer codes.
    /// </summary>
    internal class DecryptCloudRepositoryStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            try
            {
                var settingsService = serviceProvider.GetService<ISettingsService>();
                var noteRepositoryUpdater = serviceProvider.GetService<INoteRepositoryUpdater>();
                var languageService = serviceProvider.GetService<ILanguageService>();

                // Try to decode with all possible transfer codes
                bool successfullyDecryptedRepository = TryDecryptWithAllTransferCodes(
                    model, settingsService, model.BinaryCloudRepository, out byte[] decryptedRepository);

                if (successfullyDecryptedRepository)
                {
                    // Deserialize and update repository
                    XDocument cloudRepositoryXml = XmlUtils.LoadFromXmlBytes(decryptedRepository);
                    if (noteRepositoryUpdater.IsTooNewForThisApp(cloudRepositoryXml))
                        throw new SynchronizationStoryExceptions.UnsuportedRepositoryRevisionException();

                    noteRepositoryUpdater.Update(cloudRepositoryXml);
                    model.CloudRepository = XmlUtils.DeserializeFromXmlDocument<NoteRepositoryModel>(cloudRepositoryXml);

                    // Continue with next step
                    return ToTask(ToResult(new IsSameRepositoryStep()));
                }
                else
                {
                    bool existsUserEnteredTransferCode = !string.IsNullOrEmpty(model.UserEnteredTransferCode);
                    if (existsUserEnteredTransferCode)
                    {
                        // Keep transfercode page open and show message
                        if (uiMode.HasFlag(StoryMode.BusyIndicator))
                            serviceProvider.GetService<IFeedbackService>().SetBusyIndicatorVisible(false, true);
                        return ToTask(ToResultEndOfStory(languageService["sync_error_transfercode"], null));
                    }
                    else
                    {
                        // Open transfercode page
                        return ToTask(ToResult(new ShowTransferCodeStep()));
                    }
                }
            }
            catch (Exception ex)
            {
                if (uiMode.HasFlag(StoryMode.BusyIndicator))
                    serviceProvider.GetService<IFeedbackService>().SetBusyIndicatorVisible(false, true);

                // Keep the current page open and show the error message
                return ToTask(ToResult(ex));
            }
        }

        /// <summary>
        /// Tries to decrypt the binary repository testing all known transfer codes.
        /// If the decryption is successful, the transfer code and the encryption mode is updated
        /// and saved.
        /// </summary>
        /// <param name="model">The model to persist collected information.</param>
        /// <param name="settings">The application settings.</param>
        /// <param name="binaryCloudRepository">The repository to decrypt.</param>
        /// <param name="decryptedRepository">The decrypted repository, or null if the decryption
        /// was not successful.</param>
        /// <returns>Returns true if the decryption was successful, otherwise false.</returns>
        protected static bool TryDecryptWithAllTransferCodes(SynchronizationStoryModel model, ISettingsService settingsService, byte[] binaryCloudRepository, out byte[] decryptedRepository)
        {
            bool result = false;
            decryptedRepository = null;

            SettingsModel settings = settingsService.LoadSettingsOrDefault();
            ICryptor decryptor = new Cryptor("SilentNotes", null);
            List<string> transferCodesToTry = ListTransferCodesToTry(model, settings);
            int index = 0;
            while (!result && index < transferCodesToTry.Count)
            {
                string transferCodeCandidate = transferCodesToTry[index];
                result = TryDecryptRepositoryWithTransfercode(decryptor, binaryCloudRepository, transferCodeCandidate, out decryptedRepository);
                if (result)
                {
                    // Store transfercode and encryption mode if necessary
                    if (AdoptTransferCode(settings, transferCodeCandidate))
                    {
                        settingsService.TrySaveSettingsToLocalDevice(settings);
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

        private static List<string> ListTransferCodesToTry(SynchronizationStoryModel model, SettingsModel settings)
        {
            var result = new List<string>();
            bool existsUserEnteredTransferCode = !string.IsNullOrEmpty(model.UserEnteredTransferCode);
            if (existsUserEnteredTransferCode)
            {
                result.Add(model.UserEnteredTransferCode);
            }
            else
            {
                if (!string.IsNullOrEmpty(settings.TransferCode))
                    result.Add(settings.TransferCode);
                result.AddRange(settings.TransferCodeHistory);
            }
            return result;
        }

        private static bool TryDecryptRepositoryWithTransfercode(ICryptor decryptor, byte[] binaryCloudRepository, string transferCode, out byte[] decryptedRepository)
        {
            try
            {
                decryptedRepository = decryptor.Decrypt(binaryCloudRepository, CryptoUtils.StringToSecureString(transferCode));
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
