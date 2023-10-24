// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotes.Stories.PullPushStory
{
    /// <summary>
    /// This step is an end point of the "PullPushStoryBoard". It merges the local note with the
    /// downloaded note and stores the merged repository.
    /// </summary>
    internal class StoreMergedRepositoryAndQuitStep : SilentNotes.Stories.SynchronizationStory.StoreMergedRepositoryAndQuitStep
    {
        /// <inheritdoc/>
        public override async Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            uiMode = StoryMode.Toasts;
            if (!(model is PullPushStoryModel pullPushModel))
                throw new Exception("Story requires a model of type " + nameof(PullPushStoryModel));

            var repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
            var settingsService = serviceProvider.GetService<ISettingsService>();
            var languageService = serviceProvider.GetService<ILanguageService>();
            var feedbackService = serviceProvider.GetService<IFeedbackService>();

            try
            {
                NoteRepositoryModel cloudRepository = model.CloudRepository;
                repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);

                NoteModel cloudNote = cloudRepository.Notes.FindById(pullPushModel.NoteId);
                NoteModel localNote = localRepository.Notes.FindById(pullPushModel.NoteId);
                if (localNote == null)
                {
                    throw new Exception("PullPushStory is triggered on the note dialog, so the note must exist.");
                }

                if (cloudNote == null)
                {
                    // Note does not yet exist in the cloud, or it was deleted permanently. Both
                    // cases should be rejected.
                    return ToResult(null, languageService["pushpull_error_no_cloud_note"], null);
                }

                if (cloudNote.ModifiedAt == localNote.ModifiedAt)
                {
                    // Notes are equal, nothing to sync
                    return ToResult(null, languageService["pushpull_success"], null);
                }

                // Merge repositories
                if (pullPushModel.Direction == PullPushDirection.PullFromServer)
                {
                    cloudNote.CloneTo(localNote); // this can possibly move the note to the recycling bin or reverse
                    AddSafeToOtherRepositoryIfMissing(cloudRepository, localRepository, cloudNote.SafeId);
                    repositoryStorageService.TrySaveRepository(localRepository);
                }
                else
                {
                    var cryptoRandomService = serviceProvider.GetService<ICryptoRandomService>();
                    var cloudStorageClientFactory = serviceProvider.GetService<ICloudStorageClientFactory>();
                    SettingsModel settings = settingsService.LoadSettingsOrDefault();
                    SerializeableCloudStorageCredentials credentials = settings.Credentials;

                    // Uploading explicitely can be seen as a confirmation that this version is the
                    // most current one. So we make sure that the uploaded version is not overwritten
                    // by other devices afterwards.
                    localNote.RefreshModifiedAt();
                    repositoryStorageService.TrySaveRepository(localRepository);

                    localNote.CloneTo(cloudNote); // this can possibly move the note to the recycling bin or reverse
                    AddSafeToOtherRepositoryIfMissing(localRepository, cloudRepository, localNote.SafeId);
                    byte[] encryptedRepository = EncryptRepository(
                        cloudRepository, settings.TransferCode, cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                    ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);
                    await cloudStorageClient.UploadFileAsync(NoteRepositoryModel.RepositoryFileName, encryptedRepository, credentials);
                }
                return ToResult(null, languageService["pushpull_success"], null);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                return ToResult(ex);
            }
        }

        private static void AddSafeToOtherRepositoryIfMissing(NoteRepositoryModel myRepository, NoteRepositoryModel otherRepository, Guid? safeId)
        {
            bool isMissingInOtherRepository = (safeId != null) && (otherRepository.Safes.FindById(safeId) == null);
            if (isMissingInOtherRepository)
            {
                SafeModel mySafe = myRepository.Safes.FindById(safeId);
                if (mySafe != null)
                    otherRepository.Safes.Add(mySafe.Clone());
            }
        }
    }
}
