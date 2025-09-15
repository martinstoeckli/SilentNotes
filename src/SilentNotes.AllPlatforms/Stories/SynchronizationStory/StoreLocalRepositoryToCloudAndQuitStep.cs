// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step is an end point of the <see cref="SynchronizationStory"/>. It keeps the
    /// local repository and stores it to the cloud.
    /// </summary>
    internal class StoreLocalRepositoryToCloudAndQuitStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override async Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            System.Diagnostics.Debug.WriteLine("** " + nameof(StoreLocalRepositoryToCloudAndQuitStep) + " " + uiMode.ToString());

            try
            {
                var cloudStorageClientFactory = serviceProvider.GetService<ICloudStorageClientFactory>();
                var repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
                var settingsService = serviceProvider.GetService<ISettingsService>();
                var cryptoRandomService = serviceProvider.GetService<ICryptoRandomService>();
                var languageService = serviceProvider.GetService<ILanguageService>();

                SerializeableCloudStorageCredentials credentials = model.Credentials;
                repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = settingsService.LoadSettingsOrDefault();
                string transferCode = settings.TransferCode;

                bool needsNewTransferCode = !TransferCode.IsCodeSet(transferCode);
                if (needsNewTransferCode)
                    transferCode = TransferCode.GenerateCode(cryptoRandomService);

                byte[] encryptedRepository = EncryptRepository(
                    localRepository, transferCode, cryptoRandomService, settings.SelectedEncryptionAlgorithm, settings.SelectedKdfAlgorithm);

                ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);
                await cloudStorageClient.UploadFileAsync(NoteRepositoryModel.RepositoryFileName, encryptedRepository, credentials);

                // All went well, time to save the transfer code, if a new one was created
                string message = null;
                if (needsNewTransferCode)
                {
                    settings.TransferCode = transferCode;
                    settings.NotificationTriggers.Add(new NotificationTriggerModel { Id = NotificationService.TransferCodeNotificationId });
                    settingsService.TrySaveSettingsToLocalDevice(settings);

                    string formattedTransferCode = TransferCode.FormatTransferCodeForDisplay(transferCode);
                    string messageNewCreated = languageService.LoadTextFmt("transfer_code_created", formattedTransferCode, languageService.LoadText("show_transfer_code"));
                    string messageWriteDown = languageService.LoadText("transfer_code_writedown");
                    message = messageNewCreated + Environment.NewLine + messageWriteDown;
                }

                return ToResult(new StopAndShowRepositoryStep(), languageService["sync_success"], message);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                return ToResult(ex);
            }
        }
    }
}
