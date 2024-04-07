// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;
using Microsoft.Extensions.DependencyInjection;
using VanillaCloudStorageClient;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step is an end point of the <see cref="SynchronizationStory"/>. It merges the
    /// local repository with the downloaded repository and stores the merged repository.
    /// </summary>
    internal class StoreMergedRepositoryAndQuitStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override async Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            System.Diagnostics.Debug.WriteLine("** " + nameof(StoreMergedRepositoryAndQuitStep) + " " + uiMode.ToString());

            try
            {
                var settingsService = serviceProvider.GetService<ISettingsService>();
                var repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
                var languageService = serviceProvider.GetService<ILanguageService>();
                var cryptoRandomService = serviceProvider.GetService<ICryptoRandomService>();
                var cloudStorageClientFactory = serviceProvider.GetService<ICloudStorageClientFactory>();

                repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = settingsService.LoadSettingsOrDefault();

                // Merge repositories
                NoteRepositoryMerger merger = new NoteRepositoryMerger();
                NoteRepositoryModel mergedRepository = merger.Merge(localRepository, model.CloudRepository);

                // Store merged repository locally when different
                if (!RepositoriesAreEqual(mergedRepository, localRepository))
                {
                    repositoryStorageService.TrySaveRepository(mergedRepository);
                }

                // Store merged repository to the cloud when different, otherwise spare the slow upload
                if (!RepositoriesAreEqual(mergedRepository, model.CloudRepository))
                {
                    byte[] encryptedRepository = EncryptRepository(
                        mergedRepository, settings.TransferCode, cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                    ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(model.Credentials.CloudStorageId);
                    await cloudStorageClient.UploadFileAsync(NoteRepositoryModel.RepositoryFileName, encryptedRepository, model.Credentials);
                }

                return ToResult(new StopAndShowRepositoryStep(), languageService["sync_success"]);
            }
            catch (Exception ex)
            {
                if (uiMode.HasFlag(StoryMode.BusyIndicator))
                    serviceProvider.GetService<IFeedbackService>().SetBusyIndicatorVisible(false, true);

                // Keep the current page open and show the error message
                return ToResult(ex);
            }
        }

        private static bool RepositoriesAreEqual(NoteRepositoryModel repository1, NoteRepositoryModel repository2)
        {
            return repository1.GetModificationFingerprint() == repository2.GetModificationFingerprint();
        }
    }
}
